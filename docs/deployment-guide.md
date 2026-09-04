# Deployment & Operations Guide

This document describes how the Shopping Cart platform is deployed to AWS, how the
CI/CD pipeline is wired up, and how to promote a release from `sit` → `uat` → `prd`.
See [`docs/architecture.md`](architecture.md) for the Mermaid infrastructure and
pipeline diagrams that accompany this guide.

> **Sandbox note**: This guide, the CloudFormation templates, and the GitHub Actions
> workflows were authored and syntax-validated in a sandboxed development environment
> with **no live AWS credentials and no GitHub remote configured**. Every workflow file
> has been checked for valid YAML syntax and every CloudFormation template has been
> checked for valid YAML/intrinsic-function syntax, but **none of them have been
> executed end-to-end against a real AWS account or a real GitHub repository**. Before
> the first real deployment, follow the one-time setup steps below and run
> `deploy-infra.yml` against `sit` first to catch any environment-specific issues
> (account limits, region availability, IAM permission gaps, etc.) that can't be
> caught by static validation alone.

## 1. Architecture Recap

- 9 .NET 10 microservices, each in its own Docker image, deployed to a single shared
  **ECS Fargate** cluster behind a single shared **Application Load Balancer** using
  path-based routing (`/api/auth/*`, `/api/catalog/*`, …).
- One shared **Aurora PostgreSQL Serverless v2** cluster, one schema per service.
- **SNS → SQS** fan-out for the `OrderPlaced` / `OrderStatusChanged` events consumed by
  Inventory, Notification, and Recommendation services.
- React SPA hosted on **S3 + CloudFront** (OAC, no public bucket access).
- All infrastructure is defined in `infra/cloudformation/*.yaml`, parameterized per
  environment via `infra/cloudformation/parameters/{sit,uat,prd}/*.json`.
- All CI/CD is defined in `.github/workflows/*.yml` (see [CI/CD Pipeline](#3-cicd-pipeline)).
- Every ECS task's `ContainerImage` is pinned to an immutable `@sha256:<digest>` — never
  a mutable tag — see [Digest-only image policy](#7-digest-only-image-policy).

See [`docs/architecture.md`](architecture.md) for the full infrastructure and
pipeline-flow diagrams.

## 2. One-Time AWS + GitHub Setup

### 2.1 AWS IAM user + access key (per environment)

The workflows authenticate to AWS using a static **IAM User** access key/secret (CLI
profile style) rather than OIDC role assumption — this repo currently has no way to
provision an OIDC trust relationship, so `aws-actions/configure-aws-credentials` is
configured with `aws-access-key-id` / `aws-secret-access-key` instead of
`role-to-assume`. For each environment:

1. Create (or confirm) an IAM User dedicated to CI/CD for the environment (e.g.
   `github-actions-shopping-cart-sit`).
2. Attach a deploy policy covering: CloudFormation (full stack lifecycle),
   ECR (push/pull), ECS (register task defs, update services), IAM (`PassRole` +
   manage the specific task/execution roles this stack creates), EC2/VPC (network
   stack), RDS (Aurora cluster), Secrets Manager, SNS/SQS, S3, CloudFront, and
   Application Auto Scaling. Scope resource ARNs to the `${environment}-shopping-cart-*`
   naming convention used throughout the templates wherever possible.
3. Create an access key for the user (Security credentials → Access keys → CLI). Store
   the access key id and secret access key as described in 2.2 — never commit them to
   the repo, and rotate them periodically.

### 2.2 GitHub Environments

Create three **GitHub Environments** under repo Settings → Environments: `sit`,
`uat`, `prd`.

For each environment, set:

| Type | Name | Value |
|---|---|---|
| Variable | `AWS_ACCESS_KEY_ID` | Access key id of the IAM user created in 2.1 for this environment |
| Secret | `AWS_SECRET_ACCESS_KEY` | Secret access key of the IAM user created in 2.1 for this environment |
| Variable | `AWS_ACCOUNT_ID` | The 12-digit AWS account ID the environment deploys into |
| Variable | `AWS_REGION` | The AWS region the environment deploys into, e.g. `us-east-1` (repo-level is fine if all environments share a region; environment-level overrides it otherwise) |

Protection rules:
- **sit**: no required reviewers (fast iteration; this is the only environment
  `publish-docker-image.yml` ever deploys to directly).
- **uat**: require 1 reviewer before the `promote-docker-image.yml` dispatch run
  proceeds.
- **prd**: require 1–2 reviewers + restrict to the `main` branch.

`deploy-infra.yml` and `deploy-frontend.yml` are `workflow_dispatch`-triggered with an
`environment` choice input, and each job binds to the matching GitHub Environment via
`environment: ${{ github.event.inputs.environment }}`, so the protection rules above are
enforced automatically by GitHub. `publish-docker-image.yml` always binds to the `sit`
environment. `promote-docker-image.yml` binds to whichever environment is chosen in the
`target-environment` dispatch input (`uat` or `prd`).

> **Environment-scoping gotcha**: GitHub Actions `vars.*`/`secrets.*` referenced in a
> **workflow-level** `env:` block (defined before `jobs:`) are resolved before any job's
> `environment:` binding takes effect, so they can only ever see repository/org-level
> values — never an environment-scoped override. `AWS_REGION` (and `AWS_ACCESS_KEY_ID`)
> must therefore always be read either directly in a step's `with:`, or in a **job-level**
> `env:` block on a job that already declares `environment:`. All workflows in this repo
> follow this rule (`publish-docker-image.yml` and `promote-docker-image.yml` read
> `AWS_REGION` inside their job, not at the workflow root).

### 2.3 Repository-level variables for cross-environment promotion

`promote-docker-image.yml` copies an image from one environment's ECR straight into
another's within the *same* job, so it needs credentials for both accounts at once — a
single job can only bind to one GitHub Environment, so these must be **repository**-level
variables/secrets (Settings → Secrets and variables → Actions, *not* under any
Environment), in addition to the per-environment ones in 2.2:

| Type | Name | Value |
|---|---|---|
| Variable | `AWS_ACCOUNT_ID_SIT` / `AWS_ACCESS_KEY_ID_SIT` | Same values as the `sit` environment's `AWS_ACCOUNT_ID` / `AWS_ACCESS_KEY_ID` |
| Secret | `AWS_SECRET_ACCESS_KEY_SIT` | Same value as the `sit` environment's `AWS_SECRET_ACCESS_KEY` |
| Variable | `AWS_ACCOUNT_ID_UAT` / `AWS_ACCESS_KEY_ID_UAT` | Same values as the `uat` environment's `AWS_ACCOUNT_ID` / `AWS_ACCESS_KEY_ID` |
| Secret | `AWS_SECRET_ACCESS_KEY_UAT` | Same value as the `uat` environment's `AWS_SECRET_ACCESS_KEY` |
| Variable | `AWS_ACCOUNT_ID_PRD` / `AWS_ACCESS_KEY_ID_PRD` | Same values as the `prd` environment's `AWS_ACCOUNT_ID` / `AWS_ACCESS_KEY_ID` |
| Secret | `AWS_SECRET_ACCESS_KEY_PRD` | Same value as the `prd` environment's `AWS_SECRET_ACCESS_KEY` |

> Note: the env-suffixed names above (`_SIT`/`_UAT`/`_PRD`) must be uppercase — the
> workflow uppercases the target/source environment name (`sit`→`SIT`) to look up the
> matching `AWS_SECRET_ACCESS_KEY_<ENV>` secret dynamically.

### 2.4 ECR repositories & Aurora credentials

These are created *by* the pipeline itself (`ecr.yaml`, `database.yaml`), not manually —
just make sure the IAM user from 2.1 has permission to create them the first time.

## 3. CI/CD Pipeline

Backend and frontend each have their own independent CI → deploy pipeline; only
release tagging is shared (one semver tag/release covers the whole repo):

```
ci-backend.yml   ─┐                       ┌─▶ publish-docker-image.yml (SIT, if backend/ changed)
                   ├─▶ reusable-release-tag.yml (release published) ┤
ci-frontend.yml  ─┘                       └─▶ deploy-frontend.yml (SIT, if frontend/ changed)
```

Promotion beyond SIT stays manual: `promote-docker-image.yml` (backend, SIT→UAT→PRD)
and `deploy-frontend.yml` dispatched with `environment: uat`/`prd` (frontend).

| Workflow | Trigger | Purpose |
|---|---|---|
| `ci-backend.yml` | `pull_request` / `push` → `main` | `detect-changes` (path filter on `backend/**`) gates: build & unit test (9 services), integration tests, CodeQL (C#), and a Summary job. Always triggers (so required PR checks resolve even when skipped). On `push` to `main` only, chains into `reusable-release-tag.yml`. |
| `ci-frontend.yml` | `pull_request` / `push` → `main` | `detect-changes` (path filter on `frontend/**`) gates: build/lint/unit test, e2e tests, CodeQL (JS/TS), and a Summary job. Same always-triggers behavior. On `push` to `main` only, also chains into `reusable-release-tag.yml`. |
| `security-scan.yml` | `pull_request` / `push` → `main` | Repo-wide, ungated: Trivy filesystem scan, Trivy IaC scan (`infra/cloudformation`), gitleaks secret scan. |
| `reusable-release-tag.yml` | called (by `ci-backend.yml` and `ci-frontend.yml`, on `push` to `main`) | Parses Conventional Commits since the last tag and creates a new semver git tag + GitHub Release (major/minor/patch per `feat`/`fix`/`!`/`BREAKING CHANGE`). No-ops if only `chore`/`docs`/etc. commits are found. Both callers share the `release-tagging` concurrency group so only one actually creates the tag; the other sees it already exists and no-ops. |
| `publish-docker-image.yml` | `release: published` (or manual dispatch with a `release-tag`) | On a `release` event, `detect-changes` diffs `backend/` between this tag and the previous one and skips entirely if nothing changed. Otherwise builds, secret-scans (gitleaks), vulnerability-scans (Trivy), and pushes all 9 service images to the **SIT** ECR repositories, then deploys SIT's `ecs-service` stacks pinned to the resulting image digest. A manual `workflow_dispatch` always runs regardless of change detection. |
| `promote-docker-image.yml` | `workflow_dispatch` (`target-environment`: `uat`\|`prd`, `release-tag`) | Copies the already-published image manifest (no rebuild) from SIT→UAT or UAT→PRD via `docker buildx imagetools create`, re-scans it (Trivy + gitleaks), and deploys the target `ecs-service` stacks pinned to the re-resolved digest. |
| `reusable-dotnet-ci.yml` | called | restore/build/test one .NET service; optionally build+push its Docker image to ECR |
| `reusable-frontend-ci.yml` | called | `npm ci`/lint/test/build the React app; optionally sync to S3 + invalidate CloudFront |
| `reusable-cfn-deploy.yml` | called | `aws cloudformation deploy` for one template + one parameter file, with optional overrides |
| `deploy-infra.yml` | `workflow_dispatch` | deploys all 9 CloudFormation stacks for the chosen environment, in dependency order (see below) |
| `deploy-frontend.yml` | `release: published` (auto, SIT) or `workflow_dispatch` (any environment) | On a `release` event, `detect-changes` diffs `frontend/` between this tag and the previous one and skips entirely if nothing changed; otherwise reads the `s3-cloudfront-frontend` + `alb` stack outputs for **SIT**, builds the frontend with the live ALB URL baked in, syncs to S3, invalidates CloudFront. A manual `workflow_dispatch` (any of `sit`/`uat`/`prd`) always runs regardless of change detection. |
| `codeql.yml` | `push`/`pull_request` → `main`, weekly cron | Deeper, scheduled CodeQL scan (in addition to the per-PR `code-scanning-backend`/`code-scanning-frontend` jobs) |

See [`docs/architecture.md`](architecture.md#cicd-pipeline-flow) for the visual flow of
the pipeline.

### Deploy order (`deploy-infra.yml`)

```
network
  ├── database
  ├── ecr
  ├── ecs-cluster
  ├── sns-sqs
  ├── iam
  ├── alb
  └── s3-cloudfront-frontend
        (after database, ecr, ecs-cluster, sns-sqs, iam, alb)
        └── ecs-service × 9 (auth, catalog, cart, order, payment,
                              inventory, notification, review, recommendation)
```

The very first `deploy-infra.yml` run for a new environment will deploy the 9
`ecs-service` stacks with the placeholder `ContainerImage` value
(`REPLACED_AT_DEPLOY_TIME_BY_CI`) from the checked-in parameter files. Those ECS
services will fail to pull that image and will show as unhealthy — this is expected.
Run `publish-docker-image.yml` immediately afterward (SIT) to push real images and
update every `ecs-service` stack with a valid, digest-pinned `ContainerImage`.

### Typical first-deploy sequence for a brand-new environment

1. Run `deploy-infra.yml` with `environment: sit`.
2. Merge a change to `main` — `ci-backend.yml`/`ci-frontend.yml` run, and (if the
   commits since the last tag warrant a release) create a new release tag/GitHub
   Release.
3. `publish-docker-image.yml` and `deploy-frontend.yml` both fire automatically on
   that release, each independently skipping itself (via `detect-changes`) if its
   side (`backend/` or `frontend/`) didn't change since the previous tag. On a first
   release (or whenever both changed), this builds+pushes all 9 images to SIT and
   deploys the frontend to S3/CloudFront against the live ALB DNS name.
4. Smoke test (see [Section 4](#4-smoke-test-procedure)).

If you need to (re)deploy the frontend to `sit` outside of a release (e.g. to pick up
an infra-only ALB DNS change), run `deploy-frontend.yml` manually with
`environment: sit` — a manual dispatch always deploys regardless of change detection.

For subsequent releases, only `promote-docker-image.yml` needs to be run manually
(`target-environment: uat`, then later `target-environment: prd`) with the release tag
that was validated in the lower environment.

## 4. Smoke Test Procedure

After a `sit` deployment, verify the full user journey end-to-end against the
CloudFront URL:

1. **Browse**: load the homepage, confirm the product grid and categories render
   (data from Catalog Service via the ALB).
2. **Register/Login**: create an account, confirm JWT-based session persists
   (Auth Service).
3. **Add to cart**: add 1-2 products, confirm the cart badge/count updates
   (Cart Service, calling Catalog Service internally for product snapshot data).
4. **Checkout**: complete the simulated checkout form, confirm the order is placed
   (Order Service orchestrating Inventory reserve → Payment authorize → Inventory
   commit → `OrderPlaced` event published to SNS).
5. **Track order**: visit order history/detail, confirm status is `Placed`; as an
   admin, advance the status via `AdminOrdersPage` and confirm the customer's order
   detail page timeline updates.
6. **Post-purchase side effects**: confirm a notification appears in the
   `NotificationsBell` (Notification Service consumed the `OrderPlaced` SQS message)
   and that the purchased product appears in "trending"/"related" recommendations
   after a few purchases accumulate (Recommendation Service).
7. **Review**: leave a product review, confirm it appears in the product detail page's
   review list and average rating.

## 5. Environment Promotion Path (sit → uat → prd)

Promotion is **image-digest-driven, not rebuild-driven** — there is no code branching
per environment, and after `publish-docker-image.yml` builds an image once for SIT, it
is never rebuilt. The exact same CloudFormation templates and the exact same Docker
image (same content digest) are deployed to every environment; only the values in
`infra/cloudformation/parameters/{sit,uat,prd}/*.json` differ:

| Setting | sit | uat | prd |
|---|---|---|---|
| VPC CIDR | `10.0.0.0/16` | `10.1.0.0/16` | `10.2.0.0/16` |
| Aurora capacity (ACU min/max) | 0.5 / 2 | 0.5 / 4 | 1 / 8 |
| Aurora backup retention | 1 day | 7 days | 14 days |
| Aurora deletion protection | off | off | **on** |
| ECS task CPU / memory | 256 / 512 | 256 / 512 | 512 / 1024 |
| ECS desired count | 1 | 1 | 2 |
| ECS autoscaling min/max | 1 / 2 | 1 / 3 | 2 / 6 |
| CloudWatch log retention | 14 days | 30 days | 90 days |

To promote a release that has been verified in `sit`:

1. Confirm `ci-backend.yml` and `ci-frontend.yml` passed on the merge commit and note
   the release tag that `reusable-release-tag.yml` created (e.g. `v1.4.0`).
2. Confirm `publish-docker-image.yml` (and, if frontend changed, `deploy-frontend.yml`)
   completed successfully for that tag against `sit` and run the
   [smoke test](#4-smoke-test-procedure) there.
3. Run `promote-docker-image.yml` with `target-environment: uat` and
   `release-tag: v1.4.0` — this copies the already-scanned SIT image manifest into UAT
   (no rebuild) and deploys UAT's `ecs-service` stacks pinned to the re-resolved digest.
4. Repeat the [smoke test](#4-smoke-test-procedure) against the `uat` CloudFront URL.
5. Once UAT is signed off, run `promote-docker-image.yml` again with
   `target-environment: prd` and the same `release-tag` — subject to the `prd` GitHub
   Environment's required-reviewer protection rule.
6. If infra or frontend also changed, additionally run `deploy-infra.yml` /
   `deploy-frontend.yml` with the corresponding `environment`.

No application code, Dockerfile, or CloudFormation template differs between
environments — only the parameter JSON files, the GitHub Environment used, and (for
promotion) the source/destination ECR repository differ.

## 6. Local tooling used to build this pipeline

Per the project's setup, the following were installed locally and used while building
this pipeline (see `.agents/skills/conventional-commit/SKILL.md` for the installed
skill content):

```bash
gh skill install github/awesome-copilot conventional-commit --scope project --agent github-copilot
copilot plugin install aws-cloud-development@awesome-copilot
```

- The **conventional-commit** skill documents the Conventional Commits type taxonomy
  (`feat`, `fix`, `docs`, `style`, `refactor`, `perf`, `test`, `build`, `ci`, `chore`,
  `revert`, and `!`/`BREAKING CHANGE` for breaking changes) that
  `reusable-release-tag.yml`'s version-bump logic is built on.
- The **aws-cloud-development** plugin (installed globally under
  `~/.copilot/installed-plugins/awesome-copilot/aws-cloud-development`) provides
  CloudFormation/AWS authoring guidance for the `copilot` CLI.

## 7. Digest-only image policy

Every `ecs-service` CloudFormation stack's `ContainerImage` parameter must be a full
`<repository>@sha256:<digest>` URI — **never** a mutable tag such as `:latest` or
`:v1.4.0`. This is enforced structurally, not just by convention:

- `publish-docker-image.yml` resolves the digest via `aws ecr describe-images` right
  after pushing, and passes `ContainerImage=<repo>@<digest>` to
  `aws cloudformation deploy` — the release tag itself is only ever used to *find* the
  image, never to *run* it.
- `promote-docker-image.yml` copies the manifest with
  `docker buildx imagetools create` (a byte-for-byte copy, not a rebuild), then
  re-resolves the **destination** repository's digest before deploying, so each
  environment's ECS tasks always pull the exact, immutable content that was scanned in
  that promotion run.
- `infra/cloudformation/ecs-service.yaml`'s `ContainerImage` parameter description
  spells out this requirement explicitly for anyone deploying the stack manually.

## 8. Verification Status of This Pipeline

The following has been verified in this development sandbox:

- ✅ All 9 CloudFormation templates parse as valid YAML with correctly-formed
  CloudFormation intrinsic functions.
- ✅ All 51 parameter JSON files (9 templates × sit/uat/prd, with `ecs-service`
  further split per-microservice) are valid JSON in the AWS CLI parameter-file format.
- ✅ All 10 GitHub Actions workflow files parse as valid YAML.
- ✅ Full .NET backend (21 projects) builds clean and all unit tests pass locally.
- ✅ Frontend builds, lints, and all Vitest unit tests pass locally.

The following has **NOT** been verified (requires a real AWS account + GitHub remote,
neither of which exist in this sandbox) and should be the first things checked when
this repository is connected to real infrastructure:

- ⬜ `aws cloudformation validate-template` / `cfn-lint` against a real AWS account
  (catches things static YAML parsing cannot, e.g. invalid property names/values for
  a given resource type, circular exports, IAM policy syntax errors).
- ⬜ An actual `deploy-infra.yml` run — first-time stack creation may surface issues
  like account service-quota limits, region availability of specific instance/engine
  versions, or IAM permission gaps in the deploy role.
- ⬜ An actual `publish-docker-image.yml` run — Docker builds inside GitHub-hosted
  runners, Trivy/gitleaks scanning, ECR push, digest resolution, and the resulting ECS
  service update/rollout.
- ⬜ An actual `promote-docker-image.yml` run — cross-account `docker buildx imagetools
  create`, especially if `sit`/`uat`/`prd` are genuinely separate AWS accounts (verify
  the source ECR repository policy allows the destination account's role to pull).
- ⬜ An actual `deploy-frontend.yml` run — S3 sync + CloudFront invalidation, and the
  built frontend's `VITE_API_BASE_URL` correctly pointing at the live ALB.
- ⬜ The full smoke test in [Section 4](#4-smoke-test-procedure) against a live `sit`
  deployment.
- ⬜ GitHub Environment protection rules (reviewer gates on uat/prd) — must be
  configured manually in the repository's Settings UI as described in
  [Section 2.2](#22-github-environments); this cannot be scripted from this sandbox.
