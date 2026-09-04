# CI/CD with GitHub Actions

## Authentication: OIDC, not static keys

Configure an IAM role with a trust policy for GitHub's OIDC provider (`token.actions.githubusercontent.com`), scoped to this repo/branch via the `sub` claim. The workflow then uses `aws-actions/configure-aws-credentials` with `role-to-assume` — no `AWS_ACCESS_KEY_ID`/`AWS_SECRET_ACCESS_KEY` secrets stored in GitHub at all.

Reference: [Microsoft Docs / AWS docs — configuring OIDC for GitHub Actions] (search official AWS documentation for "GitHub Actions OpenID Connect IAM role" for the exact trust-policy JSON — verify the audience and subject claims match your repo before applying).

## Pipeline Stages

See [deploy.yml](../templates/deploy.yml):

1. **Build & test** — run the app's existing test suite before building images (fail fast).
2. **Build & push images** — build frontend/backend Docker images, tag with the git SHA (not `latest` — enables rollback), push to ECR.
3. **Deploy** — either:
   - `npx cdk deploy` if infrastructure or task definitions changed, or
   - `aws ecs update-service --service <name> --force-new-deployment` for image-only updates (faster, no CDK diff needed).
4. **Post-deploy check** — hit the ALB health endpoint and fail the job if it doesn't return 200 within a timeout, so bad deploys are caught before the job goes green.

## Environments and Approvals

- Use GitHub Environments (e.g., `staging`, `production`) with required reviewers on `production` so infra/deploy changes need explicit approval — mirrors the "ask before destructive/shared-system actions" principle.
- Keep `staging` deploys automatic on merge to `main`; gate `production` behind a manual approval or a tag/release trigger.

## Rollback

Tag ECR images with the commit SHA. Rolling back = re-running `aws ecs update-service` (or `cdk deploy`) pointed at the previous task definition revision — keep at least the last 5 task definition revisions active (ECS retains these automatically; don't deregister them prematurely).
