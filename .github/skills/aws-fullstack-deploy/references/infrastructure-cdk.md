# Infrastructure with AWS CDK

Use TypeScript CDK (`aws-cdk-lib`) so infrastructure changes go through the same PR review as app code.

## Resources to Provision

1. **VPC** — 2 AZs, public subnets (ALB, NAT) + private subnets (ECS tasks, RDS). Avoid NAT gateways per-AZ unless HA is required (cost driver).
2. **ECR repositories** — one per image (frontend, backend). Enable image scanning on push.
3. **ECS Cluster** — Fargate capacity, no EC2 instances to manage.
4. **Task Definitions** — separate CPU/memory per service; backend gets a **task role** (app-level AWS permissions, e.g., S3/SQS access) distinct from the **execution role** (pulls image, writes logs, reads secrets).
5. **Fargate Services** — desired count ≥ 2 for the backend in production (rolling deploys need headroom); attach to ALB target groups via health checks on a real endpoint (e.g., `/health`), not `/`.
6. **Application Load Balancer** — path-based routing: `/api/*` → backend target group, default → frontend target group. HTTPS listener with an ACM certificate; HTTP listener redirects to HTTPS.
7. **Secrets Manager / SSM Parameter Store** — connection strings, API keys. Reference them in the task definition's `secrets` (not `environment`) so values aren't visible in `DescribeTaskDefinition` output/logs.
8. **RDS (optional)** — private subnets only, security group allowing inbound only from the backend service's security group.
9. **CloudWatch Log Groups** — one per service, retention set explicitly (default is "never expire" — set 30/90 days per cost/compliance needs).

## Adapting the Template

Start from [cdk-stack-template.ts](../templates/cdk-stack-template.ts):
- Replace placeholder image tags/repo names.
- Confirm the health-check path matches the app's actual health endpoint.
- Set `desiredCount` and `cpu`/`memory` based on the app's real load — don't guess for production; start conservative (256 CPU/512MB) and adjust from ECS metrics.
- Add the RDS construct only if the assessment in step 1 found a database dependency.

## Deploying

```bash
npm install
npx cdk bootstrap aws://<ACCOUNT_ID>/<REGION>   # once per account/region
npx cdk diff     # review changes before applying
npx cdk deploy
```

Never run `cdk deploy` against a shared/production account without showing the user the `cdk diff` output first — treat it like the `terraform plan` / `what-if` step in other clouds.
