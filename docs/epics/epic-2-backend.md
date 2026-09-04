# Epic 2 — Backend development (.NET v10)

**Tech stack:** ASP.NET Core Web API microservices (.NET 10)

One microservice per bounded context (Auth, Catalog, Cart, Order, Inventory, Payment,
Notification, Recommendation, Review), each owning its schema in a shared Aurora
PostgreSQL cluster, coordinated via the checkout saga and an SNS/SQS event fan-out for
asynchronous side effects.

- **Stories**: [`../stories/PDLC-1001-backend/story.md`](../stories/PDLC-1001-backend/story.md)
- **Plan**: [`../plans/PDLC-1001-backend/plan.md`](../plans/PDLC-1001-backend/plan.md)
