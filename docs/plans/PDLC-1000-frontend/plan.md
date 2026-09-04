# Epic 1 — Frontend development (React.js v19)

> Part of the [Implementation Plan](../README.md) overview. Epic definition:
> [`../../epics/epic-1-frontend.md`](../../epics/epic-1-frontend.md). Stories:
> [`../../stories/PDLC-1000-frontend/story.md`](../../stories/PDLC-1000-frontend/story.md).

### Phase 1F — Identity & Catalog UI

- `LoginPage`, `RegisterPage`, `HomePage` (product grid), `ProductDetailPage`,
  `AdminProductsPage`.
- Exit criteria: a Shopper can register, log in, and see products on the homepage and
  their detail pages; an Admin's product management screens are inaccessible to
  Shoppers.

### Phase 2F — Cart UI

- `CartPage`, cart badge/count in the global nav.
- Exit criteria: a logged-in Shopper can add multiple products to their cart, adjust
  quantities, and remove items, with the cart persisting across page reloads.

### Phase 3F — Checkout UI

- `CheckoutPage`, `OrderConfirmationPage`.
- Exit criteria: a Shopper can complete checkout end-to-end from a non-empty cart and
  land on an order confirmation page.

### Phase 4F — Order Tracking & Admin Fulfillment UI

- `OrderHistoryPage`, `OrderDetailPage`, `AdminOrdersPage`.
- Exit criteria: a Shopper sees their placed order in history and can view its detail;
  an Admin can advance its status via `AdminOrdersPage`.

### Phase 5F — Notifications & Recommendations UI

- `NotificationsBell` component; recommendations surfaced on `ProductDetailPage`.
- Exit criteria: placing/updating an order produces a visible notification, and
  recommendations appear on product detail pages once enough orders accumulate.

### Phase 6F — Reviews UI

- Review form + list on `ProductDetailPage`.
- Exit criteria: a Shopper can leave a review and see it (and the recomputed average
  rating) on the product detail page.
