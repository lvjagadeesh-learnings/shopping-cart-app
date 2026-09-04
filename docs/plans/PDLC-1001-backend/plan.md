# Epic 2 — Backend development (.NET v10)

> Part of the [Implementation Plan](../README.md) overview. Epic definition:
> [`../../epics/epic-2-backend.md`](../../epics/epic-2-backend.md). Stories:
> [`../../stories/PDLC-1001-backend/story.md`](../../stories/PDLC-1001-backend/story.md).

### Phase 1B — Identity & Catalog Services

- **Auth Service**: registration, login, JWT issuance/validation.
- **Catalog Service**: product CRUD (Admin-only writes), product listing/detail
  (public reads).
- Exit criteria: an Admin can create a product via the API; a Shopper can register and
  log in and retrieve the catalog.

### Phase 2B — Cart Service

- **Cart Service**: per-user cart with add/update/remove line items; calls Catalog
  Service internally to snapshot product price/name at add-time.
- Exit criteria: cart contents persist server-side across sessions for a logged-in
  Shopper.

### Phase 3B — Checkout Orchestration

- **Order Service**: owns the checkout saga — reserve inventory (call Inventory
  Service) → authorize payment (call Payment Service) → commit inventory reservation
  → persist the order → publish `OrderPlaced` to SNS. Any failure partway rolls back
  the reservation and surfaces a clear error instead of leaving inconsistent state.
- **Inventory Service**: stock levels per product, reserve/commit/release operations.
- **Payment Service**: simulated authorization (approve/decline based on simple rules
  for demo purposes — no real payment gateway, per out-of-scope).
- Exit criteria: a simulated payment decline correctly leaves no order placed and
  inventory unreserved; a successful checkout persists an order and publishes
  `OrderPlaced`.

### Phase 4B — Order Tracking & Fulfillment Services

- **Order Service**: order history/detail queries; Admin status-update endpoint that
  publishes `OrderStatusChanged` to SNS.
- Exit criteria: an Admin can advance an order's status via the API and
  `OrderStatusChanged` is published.

### Phase 5B — Asynchronous Side-Effect Services

- **SNS/SQS fan-out**: one topic (`OrderPlaced`/`OrderStatusChanged` events), one
  subscribed queue per consuming service (Inventory, Notification, Recommendation),
  each with a dead-letter queue.
- **Notification Service**: consumes both event types, creates in-app notifications.
- **Recommendation Service**: consumes `OrderPlaced`, accumulates simple
  popularity/affinity signals, exposes a "related products" endpoint.
- Exit criteria: placing an order produces a Notification Service record and,
  after enough orders accumulate, a non-empty Recommendation Service response —
  without the Order Service ever calling either service directly.

### Phase 6B — Reviews Service

- **Review Service**: create review (rating + text) per product per user; list
  reviews and compute average rating per product.
- Exit criteria: the average rating recomputes correctly after a new review is
  submitted.
