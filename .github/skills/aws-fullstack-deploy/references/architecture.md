# Choosing a Target

| Approach | Best for | Trade-offs |
|---|---|---|
| **ECS Fargate + ALB** (default) | Containerized API + SPA, predictable traffic, need for background workers/websockets | You manage task sizing; slightly more setup than Beanstalk |
| **Elastic Beanstalk** | Small teams wanting minimal AWS knowledge, single-container or simple multi-container apps | Less control over networking; slower to adopt CDK-native patterns |
| **S3 + CloudFront (frontend) + Lambda/API Gateway (backend)** | Static frontend, spiky/low-traffic API, cost-sensitive at low scale | Cold starts; 15-min Lambda timeout; not ideal for long-lived connections or heavy background jobs |
| **EC2 (self-managed)** | Existing VM-based deployment habits, need OS-level control | You own patching, scaling, and orchestration — avoid unless there's a specific reason |

## Decision Questions to Ask the User

1. Is the API stateless and fine with cold starts, or does it need persistent connections (SignalR, websockets, long-running jobs)? → Lambda is a poor fit if yes.
2. Is traffic steady or spiky/low-volume? → Spiky/low favors Lambda cost-wise; steady favors Fargate.
3. Does the team want to manage container orchestration, or prefer a more "just deploy it" experience? → Beanstalk for the latter.
4. Is there a need for a shared VPC with other AWS resources (RDS, ElastiCache)? → Fargate and Beanstalk both support this; Lambda needs VPC-attached ENIs which add cold-start latency.

Default to **ECS Fargate** unless the answers clearly point elsewhere. Document the chosen target and reasoning before moving to containerization.
