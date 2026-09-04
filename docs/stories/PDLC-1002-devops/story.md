# Epic 3 — DevOps (Workflows & Infra)

> Part of the [Product Stories](../README.md) overview. Epic definition:
> [`../../epics/epic-3-devops.md`](../../epics/epic-3-devops.md). Delivery plan:
> [`../../plans/PDLC-1002-devops/plan.md`](../../plans/PDLC-1002-devops/plan.md).

Everything that builds, secures, and ships the platform to AWS — CI/CD workflows and
Infrastructure as Code.

- **US-3.1**: As the platform, I want the application code and infrastructure
  templates to be deployable, unmodified, to three environments — `sit` (fast
  iteration/no gate), `uat` (pre-production sign-off), `prd` (production, restricted +
  reviewer-gated) — differing only by environment-scoped configuration, so that
  promotion between environments is low-risk and predictable.
- **US-3.2**: As the platform, I want every AWS resource defined in CloudFormation
  under `infra/cloudformation/`, parameterized per environment, so that environments
  can be stood up and evolved with no manual console changes.
- **US-3.3**: As the platform, once a Docker image is built and scanned for a release,
  I want that exact image (by content digest) — never a rebuild — to be what's
  promoted through every environment, and every deployment to reference the image by
  immutable digest rather than a mutable tag, so that what was scanned is exactly what
  runs in production.
- **US-3.4**: As the platform, I want every change to pass automated vulnerability
  scanning (dependencies/filesystem, IaC, container image) and secret-leak scanning
  before it can reach any environment, so that known vulnerabilities and leaked
  secrets never ship.
- **US-3.5**: As the platform, I want a 4-stage automated pipeline — **CI** (build,
  unit tests, integration/e2e tests, CodeQL, Trivy + gitleaks scanning), **Create
  Release Tag** (Conventional-Commits-driven semver + GitHub Release), **Publish
  Docker Image** (build/scan/push to `sit`, deploy pinned to digest), and **Promote
  Docker Image** (re-scan and deploy the already-built image to `uat`/`prd` on manual
  approval) — so that every release is fully automated except for the required
  promotion approvals.
- **US-3.6**: As the platform, I want releases traceable to the exact set of commits
  they contain via Conventional-Commits-driven semantic versioning and GitHub
  Releases, so that versions are never manually assigned and every release has a
  clear changelog.

## Non-Functional Requirements

- **NFR-2 (Environments)**: The platform must be deployable to three environments —
  `sit`, `uat`, `prd` — with identical application code and infrastructure templates,
  differing only by environment-scoped configuration.
- **NFR-3 (Supply-chain integrity)**: Once a Docker image is built and scanned for a
  release, that exact image (by content digest) — not a rebuild — must be what is
  promoted through every environment, and every deployment must reference the image
  by immutable digest, never a mutable tag.
- **NFR-4 (Security)**: Every change must pass automated vulnerability scanning
  (dependencies/filesystem, IaC, container image) and secret-leak scanning before it
  can reach any environment.
- **NFR-5 (Traceability)**: Releases must be traceable to the exact set of commits
  they contain via Conventional-Commits-driven semantic versioning and GitHub
  Releases, not manually-assigned version numbers.
