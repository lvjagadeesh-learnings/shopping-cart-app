# Post-Deployment Validation

Run these checks after every `cdk deploy` or CI/CD pipeline run — don't tell the user "it's deployed" until they pass.

## 1. ECS Service Health

```bash
aws ecs describe-services --cluster <cluster-name> --services <backend-service> <frontend-service> \
  --query 'services[].{name:serviceName,running:runningCount,desired:desiredCount,events:events[0:3]}'
```
`runningCount` should equal `desiredCount`. If not, check `events` for the reason (image pull failure, health check failure, insufficient capacity).

## 2. ALB Target Health

```bash
aws elbv2 describe-target-health --target-group-arn <target-group-arn>
```
All targets should show `"State": "healthy"`. `"Unhealthy"` targets usually mean the health-check path is wrong or the app isn't listening on the expected port.

## 3. CloudWatch Logs

```bash
aws logs tail /ecs/<service-name> --since 10m --follow
```
Check for startup errors, unhandled exceptions, or missing configuration/secrets.

## 4. End-to-End Request

```bash
curl -i https://<alb-dns-name>/health       # backend
curl -i https://<alb-dns-name>/             # frontend
```
Confirm HTTPS works (no cert errors) and both routes return the expected status codes.

## 5. Cost Sanity Check

Summarize the primary cost drivers for the user: Fargate vCPU/memory-hours, ALB hourly + LCU charges, NAT gateway hourly + data processing, RDS instance (if provisioned), CloudWatch log storage. Flag anything that looks oversized for the app's actual traffic (e.g., desired count of 4 for a low-traffic internal tool).
