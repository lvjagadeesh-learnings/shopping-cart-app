---
name: aws-fullstack-deploy
description: 'Deploy a full-stack app (React frontend + ASP.NET Core/.NET API backend) to AWS using containers, ECS Fargate, and AWS CDK. Use when: deploy to AWS, AWS deployment plan, containerize for AWS, push to ECR, provision AWS infrastructure, set up ECS Fargate, AWS CDK stack, ALB routing, CI/CD to AWS, full stack app AWS, ship app to AWS, AWS ECS deployment, S3 + CloudFront frontend hosting.'
argument-hint: 'Describe your app (frontend/backend stack, repo layout) and target AWS region/account'
---

# AWS Full-Stack App Deployment

Deploys a two-tier app (SPA frontend + API backend) to AWS using Docker containers on ECS Fargate, provisioned with AWS CDK, and deployed via GitHub Actions with OIDC (no long-lived AWS keys).

## When to Use

- Taking a React + ASP.NET Core (or similar) app to AWS for the first time
- Containerizing an app that currently has no Dockerfiles
- Replacing manual `aws` CLI deployments with reproducible IaC
- Setting up CI/CD that builds, pushes to ECR, and deploys to ECS

Not for: serverless-only apps better suited to pure Lambda (ask the user if a single static site + API Gateway/Lambda would be simpler — see [Choosing a Target](./references/architecture.md)), or migrations from another cloud (that's a broader migration task).

## Prerequisites

Confirm with the user (or detect in the repo) before starting:
- AWS account ID + target region
- AWS CLI, Docker, Node.js (for CDK) installed locally
- Frontend framework + build output folder (e.g., React → `build/` or `dist/`)
- Backend framework + entry point (e.g., ASP.NET Core → `dotnet publish` output)
- Whether a database is needed (RDS) or the app already has one

## Procedure

### 1. Assess the app

- Identify frontend and backend project folders and their build commands.
- Check for existing Dockerfiles, `docker-compose.yml`, or `.aws/` config — don't overwrite without confirming.
- Note any external dependencies (database, cache, queue, third-party APIs) that need AWS equivalents or `SecureString` secrets.

### 2. Choose the AWS target

Default recommendation: **ECS Fargate** behind an Application Load Balancer (ALB), with the frontend served either as its own Fargate service (nginx) or from S3 + CloudFront. See [Choosing a Target](./references/architecture.md) for when Elastic Beanstalk or a pure serverless (S3 + CloudFront + Lambda) approach fits better. Confirm the choice with the user before provisioning.

### 3. Containerize the app

Add multi-stage Dockerfiles for frontend and backend (small final images, no build tools in the runtime layer). Use the templates in [Containerization](./references/containerize.md):
- [Dockerfile.frontend](./templates/Dockerfile.frontend) — React build → nginx
- [Dockerfile.backend](./templates/Dockerfile.backend) — .NET SDK build → ASP.NET runtime

Build and run both locally to confirm they work before writing infrastructure code.

### 4. Provision infrastructure with AWS CDK

Scaffold a CDK TypeScript app that creates: VPC, ECR repos, ECS cluster, Fargate services (frontend + backend), ALB with path-based routing (`/api/*` → backend, `/*` → frontend), and Secrets Manager entries for connection strings/API keys. Follow [Infrastructure (CDK)](./references/infrastructure-cdk.md) and adapt [cdk-stack-template.ts](./templates/cdk-stack-template.ts).

Key rules:
- Never hardcode secrets or credentials in the CDK stack — reference Secrets Manager/SSM Parameter Store.
- Grant IAM roles least privilege (task execution role vs. task role are separate).
- Enable HTTPS on the ALB via ACM; redirect HTTP → HTTPS.

### 5. Set up CI/CD

Use GitHub Actions with OIDC federation to assume an AWS IAM role — no static `AWS_ACCESS_KEY_ID`/`AWS_SECRET_ACCESS_KEY` secrets. Follow [CI/CD](./references/cicd-github-actions.md) and adapt [deploy.yml](./templates/deploy.yml): build images → push to ECR → `cdk deploy` (or `aws ecs update-service --force-new-deployment`).

### 6. Validate the deployment

After `cdk deploy` or the pipeline finishes, run the checks in [Validation](./references/validation.md): ALB target health, ECS service events, CloudWatch logs, and an end-to-end HTTP request through the ALB DNS name.

## Gotchas

- **Never** commit AWS access keys as GitHub secrets for this pipeline — OIDC role assumption avoids long-lived credentials entirely; if a workflow already has `AWS_ACCESS_KEY_ID`, migrate it before adding new deploy steps.
- **`cdk bootstrap` is per account/region**, not per stack — skipping it produces a confusing "unable to resolve AWS account" error on first deploy.
- **ECS won't roll back automatically** on a failed deployment unless the service's deployment circuit breaker is enabled — set `circuitBreaker: { rollback: true }` on the `FargateService`, or bad images can leave the service stuck mid-rollout.
- **Task execution role ≠ task role.** The execution role lets ECS pull the image and write logs; the task role is what your application code assumes at runtime (e.g., to call S3). Granting app permissions to the execution role by mistake is a common over-privilege bug.
- **`environment` vs `secrets` in the task definition matters** — anything under `environment` is visible in plaintext via `DescribeTaskDefinition` and in the ECS console; connection strings and API keys must go under `secrets` (Secrets Manager/SSM) instead.
- **ALB health checks default to `/`.** If the app doesn't serve 200 on `/`, the target group reports every task unhealthy and cycles them forever — always point health checks at a real, fast, dependency-free health endpoint.

## Security Checklist

- [ ] No AWS long-lived access keys in CI — OIDC role assumption only
- [ ] Secrets in Secrets Manager/SSM, not environment variables in source or task definitions
- [ ] ALB enforces HTTPS; security groups scoped to required ports only
- [ ] ECS task role has least-privilege IAM policy (not `AdministratorAccess`)
- [ ] Container images scanned (ECR image scanning enabled)
- [ ] RDS (if used) not publicly accessible; in private subnets

## Output

At the end, summarize: AWS resources created, the ALB/CloudFront URL, how to redeploy (CI trigger or manual command), and estimated monthly cost drivers (Fargate vCPU/memory, ALB, NAT gateway, RDS if applicable).
