# Epic 3 — DevOps (Workflows & Infra)

**Tech stack:** GitHub Actions CI/CD + AWS CloudFormation (IaC)

Everything that builds, secures, and ships the platform to AWS: infrastructure as
code across `sit`/`uat`/`prd`, a 4-stage CI/CD pipeline (CI, Create Release Tag,
Publish Docker Image, Promote Docker Image), digest-pinned image promotion, and
automated vulnerability/secret scanning gates.

- **Stories**: [`../stories/PDLC-1002-devops/story.md`](../stories/PDLC-1002-devops/story.md)
- **Plan**: [`../plans/PDLC-1002-devops/plan.md`](../plans/PDLC-1002-devops/plan.md)
