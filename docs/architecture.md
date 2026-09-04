# Architecture Diagram

This diagram is the visual companion to [Section 1 — Architecture Recap](deployment-guide.md#1-architecture-recap)
of the [Deployment & Operations Guide](deployment-guide.md). It reflects exactly what
the CloudFormation templates in `infra/cloudformation/*.yaml` provision, one full stack
per environment (`sit`, `uat`, `prd`).

## High-level infrastructure

```mermaid
flowchart TB
    subgraph Client["Client"]
        Browser["Browser / SPA user"]
    end

    subgraph Edge["Edge (per environment)"]
        CF["CloudFront Distribution\n(OAC, HTTPS)"]
        S3["S3 Bucket\nfrontend static assets\n(no public access)"]
        CF --> S3
    end

    subgraph VPC["VPC — 10.x.0.0/16 (sit=10.0, uat=10.1, prd=10.2)"]
        subgraph PublicSubnets["Public Subnets (2 AZs)"]
            ALB["Application Load Balancer\npath-based routing"]
        end

        subgraph PrivateSubnets["Private Subnets (2 AZs)"]
            subgraph ECS["ECS Fargate Cluster"]
                Auth["auth-service\n/api/auth/*"]
                Catalog["catalog-service\n/api/catalog/*"]
                Cart["cart-service\n/api/cart/*"]
                Order["order-service\n/api/orders/*"]
                Payment["payment-service\n/api/payments/*"]
                Inventory["inventory-service\n/api/inventory/*"]
                Notification["notification-service\n/api/notifications/*"]
                Review["review-service\n/api/reviews/*"]
                Recommendation["recommendation-service\n/api/recommendations/*"]
            end

            Aurora[("Aurora PostgreSQL\nServerless v2\n(1 schema per service)")]
        end

        SNS["SNS Topic\nOrderPlaced / OrderStatusChanged"]
        SQS_Inv["SQS Queue\n(Inventory)"]
        SQS_Notif["SQS Queue\n(Notification)"]
        SQS_Rec["SQS Queue\n(Recommendation)"]
        DLQ["Dead-letter Queues\n(per subscriber)"]
    end

    subgraph ECR_Group["ECR (per environment)"]
        ECR["9x ECR Repositories\n<env>-<service>-service\nimages pulled by DIGEST only"]
    end

    Browser -->|HTTPS| CF
    Browser -->|HTTPS /api/*| ALB

    ALB --> Auth & Catalog & Cart & Order & Payment & Inventory & Notification & Review & Recommendation

    Auth --> Aurora
    Catalog --> Aurora
    Cart --> Aurora
    Cart -.->|internal call| Catalog
    Order --> Aurora
    Payment --> Aurora
    Inventory --> Aurora
    Notification --> Aurora
    Review --> Aurora
    Recommendation --> Aurora

    Order -->|OrderPlaced /\nOrderStatusChanged| SNS
    SNS --> SQS_Inv --> Inventory
    SNS --> SQS_Notif --> Notification
    SNS --> SQS_Rec --> Recommendation
    SQS_Inv -.-> DLQ
    SQS_Notif -.-> DLQ
    SQS_Rec -.-> DLQ

    ECR -. digest-pinned image .-> ECS
```

## CI/CD pipeline flow

```mermaid
flowchart LR
    PR["Pull Request\nor push to main"] --> CIB
    PR --> CIF
    PR --> Sec

    subgraph CIB["CI - Backend (ci-backend.yml)"]
        direction TB
        DetectB["detect-changes\n(backend/** ?)"]
        BuildB["Build & Unit Test\n(9 backend services)"]
        Integ["Integration Tests"]
        ScanB["Code Scanning\n(CodeQL - C#)"]
        SummaryB["Summary"]
        DetectB -->|changed| BuildB --> SummaryB
        DetectB -->|changed| Integ --> SummaryB
        DetectB -->|changed| ScanB --> SummaryB
    end

    subgraph CIF["CI - Frontend (ci-frontend.yml)"]
        direction TB
        DetectF["detect-changes\n(frontend/** ?)"]
        BuildF["Build, Lint & Unit Test"]
        E2E["End-to-End Tests"]
        ScanF["Code Scanning\n(CodeQL - JS/TS)"]
        SummaryF["Summary"]
        DetectF -->|changed| BuildF --> SummaryF
        DetectF -->|changed| E2E --> SummaryF
        DetectF -->|changed| ScanF --> SummaryF
    end

    subgraph Sec["Security Scan (security-scan.yml) — ungated"]
        Vuln["Vulnerability Checks\n(Trivy fs/config + gitleaks)"]
    end

    CIB -->|push to main only| RT
    CIF -->|push to main only| RT

    subgraph RT["Create Release Tag\n(reusable-release-tag.yml, shared\nrelease-tagging concurrency group)"]
        Bump["Parse Conventional Commits\nsince last tag -> bump semver"]
        Tag["git tag + GitHub Release"]
        Bump --> Tag
    end

    RT -->|release: published| Publish
    RT -->|release: published| DeployFE

    subgraph Publish["Publish Docker Image\n(publish-docker-image.yml) — SIT only"]
        direction TB
        DetectPB["detect-changes\n(backend/** since prev tag?)"]
        PBuild["docker build"]
        PVuln["Trivy image scan +\ngitleaks secret scan"]
        PPush["push to sit-*-service ECR"]
        PDigest["resolve digest"]
        PDeploy["deploy sit ecs-service\nContainerImage=@digest"]
        DetectPB -->|changed| PBuild --> PVuln --> PPush --> PDigest --> PDeploy
    end

    subgraph DeployFE["Deploy Frontend\n(deploy-frontend.yml) — auto SIT"]
        direction TB
        DetectPF["detect-changes\n(frontend/** since prev tag?)"]
        FEBuild["npm ci / lint / test / build"]
        FESync["sync to S3 +\ninvalidate CloudFront"]
        DetectPF -->|changed| FEBuild --> FESync
    end

    Publish -->|manual dispatch\ntarget=uat, release-tag| PromoteUAT


    subgraph PromoteUAT["Promote Docker Image -> UAT"]
        direction TB
        CopyU["buildx imagetools create\n(copy manifest, no rebuild)"]
        VulnU["Trivy re-scan + gitleaks"]
        DeployU["deploy uat ecs-service\nContainerImage=@digest"]
        VulnU --> CopyU --> DeployU
    end

    PromoteUAT -->|manual dispatch\ntarget=prd, release-tag| PromotePRD

    subgraph PromotePRD["Promote Docker Image -> PRD"]
        direction TB
        CopyP["buildx imagetools create\n(copy manifest, no rebuild)"]
        VulnP["Trivy re-scan + gitleaks"]
        DeployP["deploy prd ecs-service\nContainerImage=@digest"]
        VulnP --> CopyP --> DeployP
    end
```

## Notes

- **Single reusable pipeline**: `promote-docker-image.yml` is the same workflow for both
  SIT→UAT and UAT→PRD; the `target-environment` dispatch input selects the source
  environment automatically (`uat` promotes from `sit`, `prd` promotes from `uat`).
- **No rebuilds after Publish**: promotion only ever copies an already-scanned,
  already-published image manifest between ECR repositories — the same bytes that were
  validated in the lower environment are what runs in the next one.
- **Digest-only enforcement**: every `ecs-service` CloudFormation stack update (whether
  from `publish-docker-image.yml` or `promote-docker-image.yml`) sets `ContainerImage`
  to a full `@sha256:<digest>` reference, never a mutable tag, so ECS Fargate always
  pulls the exact, immutable image content that was scanned.
