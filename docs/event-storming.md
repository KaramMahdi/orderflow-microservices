# Event storming

**Draft v1 (Phase 1, Milestone 3).** Built from the aggregates and events in the plans, then aligned
with the domain model in code. Decisions are marked with their source; open points are listed at the end.

Legend: **Command** (an intent, blue sticky), **Event** (a fact in the past tense, orange sticky),
**Aggregate** (yellow), **Policy** (when X then Y, lilac), **Read model** (green), **Hot spot**
(an open question, red).

## Aggregates by context

| Context | Aggregate root | Inside | Key invariants | Domain events |
|---------|----------------|--------|----------------|---------------|
| Orders | Order | OrderItem; Address and Money (value objects) | Items given only when the order is placed (so none after payment); at least one item, one per product; total equals the sum of items; all items in the order's one currency; quantity above zero; only allowed status transitions | OrderPlaced, OrderConfirmed, OrderPaid, OrderCancelled |
| Catalog | Product | Sku; one ProductPrice (Money) per currency | SKU unique; one price per currency; price not negative; NormalizedName follows Name | ProductPriceChanged |
| Catalog | StockItem | OnHand, Reserved counter, StockMovements (Reserved, Completed, Failed) | Quantity never negative; Reserved never above on hand; Reserved equals the sum of Reserved movements; one reservation per order; a released reservation is never completed | StockReserved, StockCompleted, StockReleased |
| Customers | Customer | UserId; Email and Address (value objects) | Email valid and unique; one customer per user | CustomerRegistered |
| Payments | Payment | Money, PaymentAttempts | Amount equals the order total and is above zero; one successful payment per order; no attempt after Completed or Refunded; only a completed payment is refunded, once | PaymentCompleted, PaymentFailed, PaymentRefunded |
| Notifications | Notification | Channel, recipient (id and address), attempts, next attempt at | Attempts not above the maximum (Options); only a Pending notification is sent or retried | NotificationSent, NotificationFailed |
| Files | FileRecord | Metadata only (name, path, size, hash, owner link) | Pending becomes Active once saved and scanned (or Rejected, or Failed by the job); the unique name never changes | FileStored |

## Timeline: the main flow (place and pay an order)

| # | Actor or trigger | Command | Aggregate | Event | Context |
|---|------------------|---------|-----------|-------|---------|
| 1 | Visitor | Register | Customer (and Identity User) | CustomerRegistered | Customers, Identity |
| 2 | User | Log in | (Identity) | token issued (not a domain event) | Identity |
| 3 | Customer | Browse / search products (prefix) | read model | - | Catalog |
| 4 | Customer | Place order | Order | OrderPlaced | Orders |
| 5 | Policy: when an order is placed, reserve its stock | Reserve stock | StockItem | StockReserved | Catalog |
| 6 | Policy: when the stock of every product is reserved | Confirm order | Order | OrderConfirmed | Orders |
| 7 | Policy: when an order is created, collect the payment | Process payment | Payment | PaymentCompleted or PaymentFailed | Payments |
| 8a | Policy: when payment completed | Mark order paid | Order | OrderPaid | Orders |
| 8b | Policy: when payment completed | Complete stock movement | StockItem | StockCompleted | Catalog |
| 9 | Policy: when payment failed | Nothing is cancelled: the order keeps waiting for payment while its reservation lives | Order | - | Orders |
| 10 | Hangfire job, every 5 minutes | Mark expired reservations Failed (older than 15 minutes), then cancel their orders | StockItem, Order | StockReleased, OrderCancelled | Catalog, Orders |
| 11 | Policy: when a payment arrives after the reservation expired (the order is already Cancelled) | Refund payment (simulated in Phase 1). The order stays Cancelled | Payment | PaymentRefunded | Payments |
| 12 | Policy: when an order or payment event happens | Send notification | Notification | NotificationSent or NotificationFailed | Notifications |
| 13 | Hangfire job (Phase 3) | Generate invoice PDF | FileRecord | FileStored | Orders, Files |

## Timeline: supporting flows

| Actor or trigger | Command | Aggregate | Event | Context |
|------------------|---------|-----------|-------|---------|
| Manager | Create product | Product | *(no event listed yet)* | Catalog |
| Manager | Change product price | Product | ProductPriceChanged | Catalog |
| Manager | Adjust stock on hand | StockItem | *(no event listed yet)* | Catalog |
| User | Upload file | FileRecord | FileStored (Active) or rejected | Files |
| User | Download file | FileRecord + owner access check | - | Files |
| Hangfire job | Mark stale Pending files Failed | FileRecord | *(no event listed yet)* | Files |
| Policy: when a notification failed and attempts remain | Retry notification | Notification | NotificationSent or NotificationFailed | Notifications |

## Integration events (Phase 3, contracts in Domain.Shared.Layer)

| Integration event | Published by | Consumed by |
|-------------------|-------------|-------------|
| OrderCreated | Orders | Payments, Notifications |
| PaymentCompleted | Payments | Orders, Catalog (via Orders?), Notifications |
| PaymentFailed | Payments | Orders, Notifications |
| StockReserved | Catalog | Orders |

Catalog also publishes product changes so Orders can keep its product snapshot (context map).

## Read models

- Product list and prefix search (Catalog), paged.
- "My orders", newest first (Orders), paged.
- Operations queues: open orders by status (Orders).
- Product snapshot inside Orders (name, price at order time).
- A user's notifications (Notifications).
- Files of a business record (Files).

## Hot spots (open, not decided)

- **H1. Resolved (ADR-0001):** Placed, then Confirmed when the stock is reserved, then Paid or
  Cancelled. A failed payment does not cancel the order; an expired reservation cancels it; Cancelled
  is final and is never reopened. (Phase 4: a Placed order that Catalog cannot reserve is decided
  with R-016.)
- **H2. Resolved (ADR-0001):** a payment that arrives after the reservation expired is always
  refunded. The order stays Cancelled. The refund event is `PaymentRefunded` (decided by Karam,
  2026-11-27).
- **H3.** The cart: does it exist in Phase 1, and which context owns it?
- **H4.** Customers: its own service in Phase 4, or inside Orders? (Partly resolved: the Customer keeps
  the Identity user id, one customer per user, decided by Karam on 2026-11-27. When registration
  creates the Customer is still open.)
- **H5.** Naming: the domain event is `OrderPlaced` but the integration event is `OrderCreated`.
  One name or two?
- **H6.** Which completes the stock movement on payment: Orders telling Catalog, or Catalog
  consuming `PaymentCompleted` directly?
- **H7.** Product creation, stock adjustment and stale-file marking have no domain event listed yet.
