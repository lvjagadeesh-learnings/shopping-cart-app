# Epic 3 — DevOps (Workflows & Infra)

> Part of the [Implementation Plan](../README.md) overview. Epic definition:
> [`../../epics/epic-3-devops.md`](../../epics/epic-3-devops.md). Stories:
> [`../../stories/PDLC-1002-devops/story.md`](../../stories/PDLC-1002-devops/story.md).

### Phase 1D — Infrastructure as Code

- Author 9 CloudFormation templates under `infra/cloudformation/`: `network`,
  `database`, `ecr`, `ecs-cluster`, `iam`, `alb`, `sns-sqs`, `ecs-service` (reused per
  microservice via 9 parameter files), `s3-cloudfront-frontend`.
- Author parameter files per environment under
  `infra/cloudformation/parameters/{sit,uat,prd}/`, differing only in scale
  (capacity, retention, deletion protection) — see the promotion table in
  [`deployment-guide.md`](../../deployment-guide.md#5-environment-promotion-path-sit--uat--prd).
- Exit criteria: `deploy-infra.yml` can stand up a complete, working environment from
  an empty AWS account in dependency order (network → data/compute plumbing →
  services → frontend hosting).

### Phase 2D — CI/CD Pipeline

- **CI** (`ci-backend.yml`, `ci-frontend.yml`): build/unit test the backend services and
  the frontend independently, each gated by a `detect-changes` path filter so the
  irrelevant side is skipped when only the other changed; plus `security-scan.yml` for
  repo-wide Trivy/gitleaks checks,
  integration tests, e2e tests, CodeQL code scanning, Trivy + gitleaks vulnerability/
  secret-leak checks, and a Summary gate — on every PR and on push to `main`.
- **Create Release Tag** (`reusable-release-tag.yml`, called from `ci-backend.yml` and
  `ci-frontend.yml` on `main`):
  parse Conventional Commits since the last tag, compute the next semver bump, create
  a git tag + GitHub Release.
- **Publish Docker Image** (`publish-docker-image.yml`): on release, build, scan
  (Trivy + gitleaks), and push each service's image to the `sit` ECR repositories,
  then deploy `sit`'s `ecs-service` stacks pinned to the resulting content digest.
- **Promote Docker Image** (`promote-docker-image.yml`): on manual dispatch
  (target environment + release tag), copy the already-scanned image manifest
  (no rebuild) from `sit→uat` or `uat→prd`, re-scan, and deploy the target
  environment's `ecs-service` stacks pinned to the re-resolved digest.
- Exit criteria: a merge to `main` can flow, without further code changes, all the way
  to a `prd` deployment through nothing but automated pipeline runs plus the required
  manual promotion approvals — see the full sequence in
  [`deployment-guide.md`](../../deployment-guide.md#3-cicd-pipeline).
