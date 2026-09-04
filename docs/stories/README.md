# Product Stories — Shopping Cart Platform

> Written as the project-inception vision document (spec-kit style): what we are
> building and why, before any implementation begins. See
> [`../plans/README.md`](../plans/README.md) for how it was actually delivered, and
> [`../architecture.md`](../architecture.md) for the resulting technical architecture.

## Vision

Build a small but realistically-scoped e-commerce shopping cart platform that
demonstrates a microservices architecture end-to-end: browsing a catalog, managing a
cart, checking out with real orchestration between inventory/payment/order services,
tracking order status, receiving post-purchase notifications, discovering
recommended products, and leaving product reviews — with an admin surface for
managing products and order fulfillment.

## Personas

- **Shopper** — a customer browsing and buying products.
- **Admin** — an internal user managing the product catalog and fulfilling orders.

## Epics

This platform is delivered by exactly three epics, aligned to the tech stack. Epic
definitions (tech stack + scope) live under [`../epics/`](../epics/); each epic also
has its own story file with the full list of user stories:

- **Epic 1 — Frontend development** (React.js, v19): [definition](../epics/epic-1-frontend.md) ·
  [stories](PDLC-1000-frontend/story.md)
- **Epic 2 — Backend development** (.NET, v10): [definition](../epics/epic-2-backend.md) ·
  [stories](PDLC-1001-backend/story.md)
- **Epic 3 — DevOps** (Workflows & Infra): [definition](../epics/epic-3-devops.md) ·
  [stories](PDLC-1002-devops/story.md)

## Non-Functional Requirements

- **NFR-1 (Resilience)**: No single downstream service (Notification, Recommendation)
  should be able to block or fail the checkout path. *(Epic 2)*
- **NFR-2 (Environments)**: The platform must be deployable to three environments —
  `sit`, `uat`, `prd` — with identical application code and infrastructure templates,
  differing only by environment-scoped configuration. *(Epic 3)*
- **NFR-3 (Supply-chain integrity)**: Once a Docker image is built and scanned for a
  release, that exact image (by content digest) — not a rebuild — must be what is
  promoted through every environment, and every deployment must reference the image
  by immutable digest, never a mutable tag. *(Epic 3)*
- **NFR-4 (Security)**: Every change must pass automated vulnerability scanning
  (dependencies/filesystem, IaC, container image) and secret-leak scanning before it
  can reach any environment. *(Epic 3)*
- **NFR-5 (Traceability)**: Releases must be traceable to the exact set of commits
  they contain via Conventional-Commits-driven semantic versioning and GitHub
  Releases, not manually-assigned version numbers. *(Epic 3)*

## Out of Scope (for this iteration)

- Real payment gateway integration (Payment Service simulates authorization).
- Multi-currency / multi-region catalog pricing.
- Search relevance/ranking beyond basic catalog listing.
- Mobile native apps (web SPA only).
