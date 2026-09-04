# Implementation Plan — Shopping Cart Platform

> Written as the project-inception delivery plan (spec-kit style), phased so each phase
> produces a demonstrable, testable increment. Maps to the stories in
> [`../stories/README.md`](../stories/README.md) and the architecture in
> [`../architecture.md`](../architecture.md).

## Guiding principles

1. **Microservice-per-bounded-context**: one .NET 10 service per business capability
   (auth, catalog, cart, order, payment, inventory, notification, review,
   recommendation), each with its own schema in a shared Aurora PostgreSQL cluster.
2. **Event-driven side effects**: anything that isn't on the critical checkout path
   (notifications, recommendations, inventory adjustments beyond the initial
   reserve/commit) happens asynchronously off an SNS→SQS fan-out, never a synchronous
   call from Order Service.
3. **Infrastructure as Code from day one**: every AWS resource is defined in
   CloudFormation under `infra/cloudformation/`, parameterized per environment —
   no manual console changes.
3. **Same artifact everywhere**: one Docker image per service, built and scanned once,
   promoted by digest through `sit → uat → prd` — never rebuilt per environment.
4. **Security and quality gates are automated, not optional**: vulnerability scanning,
   secret-leak scanning, and code scanning run in CI before anything can be tagged
   for release, and again at publish/promote time before anything is deployed.

## Phase 0 — Foundations

- Scaffold the solution: one ASP.NET Core Web API project per microservice under
  `backend/services/`, shared conventions for health checks, logging, and JWT
  validation middleware. *(Epic 2)*
- Scaffold the React 19 + TypeScript + Vite frontend under `frontend/`, with a client
  for each backend service under `src/api/`, Zustand (or equivalent) store under
  `src/store/`, and route-level pages under `src/pages/`. *(Epic 1)*
- Establish local dev experience (per-service `dotnet run`, frontend `npm run dev`)
  and a shared test convention (xUnit for backend, Vitest for frontend). *(Epic 1, 2)*

## Epics

This plan is organized into 3 epic-aligned workstreams, each with its own plan file.
Frontend and backend phases for the same feature are sequenced together since each
depends on the other to be end-to-end demonstrable. Epic definitions (tech stack +
scope) live under [`../epics/`](../epics/).

- **Epic 1 — Frontend development** (React.js, v19): [definition](../epics/epic-1-frontend.md) ·
  [plan](PDLC-1000-frontend/plan.md)
- **Epic 2 — Backend development** (.NET, v10): [definition](../epics/epic-2-backend.md) ·
  [plan](PDLC-1001-backend/plan.md)
- **Epic 3 — DevOps** (Workflows & Infra): [definition](../epics/epic-3-devops.md) ·
  [plan](PDLC-1002-devops/plan.md)

## Traceability

| Workstream | Phase | Delivers |
|---|---|---|
| Epic 1 — Frontend | 1F | Identity & Catalog UI |
| Epic 1 — Frontend | 2F | Cart UI |
| Epic 1 — Frontend | 3F | Checkout UI |
| Epic 1 — Frontend | 4F | Order Tracking & Admin Fulfillment UI |
| Epic 1 — Frontend | 5F | Notifications & Recommendations UI |
| Epic 1 — Frontend | 6F | Reviews UI |
| Epic 2 — Backend | 1B | Identity & Catalog Services |
| Epic 2 — Backend | 2B | Cart Service |
| Epic 2 — Backend | 3B | Checkout Orchestration |
| Epic 2 — Backend | 4B | Order Tracking & Fulfillment Services |
| Epic 2 — Backend | 5B | Asynchronous Side-Effect Services |
| Epic 2 — Backend | 6B | Reviews Service |
| Epic 3 — DevOps | 1D | Infrastructure as Code (NFR-2) |
| Epic 3 — DevOps | 2D | CI/CD Pipeline (NFR-3, NFR-4, NFR-5) |
