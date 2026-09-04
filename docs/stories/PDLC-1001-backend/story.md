# Epic 2 — Backend development (.NET v10)

> Part of the [Product Stories](../README.md) overview. Epic definition:
> [`../../epics/epic-2-backend.md`](../../epics/epic-2-backend.md). Delivery plan:
> [`../../plans/PDLC-1001-backend/plan.md`](../../plans/PDLC-1001-backend/plan.md).

One ASP.NET Core Web API microservice per bounded context (.NET 10), each owning its
schema and exposing the endpoints the frontend and other services depend on.

- **US-2.1**: As the platform, I want an Auth Service handling registration, login,
  and JWT issuance/validation, so that Shopper sessions are secure and Admin-only
  actions can be authorized.
- **US-2.2**: As the platform, I want a Catalog Service exposing product CRUD
  (Admin-only writes) and product listing/detail (public reads), so that the frontend
  can render an accurate, up-to-date catalog.
- **US-2.3**: As the platform, I want a Cart Service managing a per-user cart with
  add/update/remove line items (snapshotting product price/name from the Catalog
  Service at add-time), so that Shoppers' carts persist reliably across sessions.
- **US-2.4**: As the platform, I want an Order Service that owns the checkout saga —
  reserve inventory → authorize payment → commit the reservation → persist the order →
  publish an `OrderPlaced` event — so that a Shopper is never charged without an order,
  or left with an order but no stock reserved, and downstream services can react
  without the Order Service knowing about them directly.
- **US-2.5**: As the platform, I want an Inventory Service exposing stock levels and
  reserve/commit/release operations per product, so that the checkout saga can
  atomically manage stock.
- **US-2.6**: As the platform, I want a Payment Service that simulates authorization
  (approve/decline for demo purposes, no real payment gateway), so that the checkout
  saga has a payment step to orchestrate against.
- **US-2.7**: As the platform, I want the Order Service to expose order history/detail
  queries and an Admin status-update endpoint that publishes `OrderStatusChanged`, so
  that fulfillment progress can be tracked and communicated.
- **US-2.8**: As the platform, I want a Notification Service that consumes
  `OrderPlaced`/`OrderStatusChanged` events off SNS/SQS and creates in-app
  notifications, decoupled from the Order Service, so that a slow or failing
  Notification Service never blocks order placement.
- **US-2.9**: As the platform, I want a Recommendation Service that consumes
  `OrderPlaced` events to accumulate popularity/affinity signals and exposes a
  "related products" endpoint, so that recommendations improve over time without
  coupling the Order Service to how they're computed.
- **US-2.10**: As the platform, I want a Review Service to create reviews
  (rating + text) per product per user and compute the average rating per product, so
  that Shoppers can share and view purchase experiences.

## Non-Functional Requirements

- **NFR-1 (Resilience)**: No single downstream service (Notification, Recommendation)
  should be able to block or fail the checkout path.
