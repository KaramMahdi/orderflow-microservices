# Event storming

**Draft v0 (Phase 0).** Built from the aggregates and events in the plans. It will be refined with
Karam's answers and during Phase 1. Nothing here is a decision; open points are listed at the end.

Legend: **Command** (an intent, blue sticky), **Event** (a fact in the past tense, orange sticky),
**Aggregate** (yellow), **Policy** (when X then Y, lilac), **Read model** (green), **Hot spot**
(an open question, red).

## Aggregates by context

| Context | Aggregate root | Inside | Key invariants | Domain events |
|---------|----------------|--------|----------------|---------------|
| Orders | Order | OrderItem; Address and Money (value objects) | No items after payment; total equals the sum of items; all items in the order's one currency; quantity above zero; only allowed status transitions | OrderPlaced, OrderConfirmed, OrderPaid, OrderCancelled |
| Catalog | Product | Sku; one ProductPrice (Money) per currency | SKU unique; one price per currency; price not negative | ProductPriceChanged |
| Catalog | StockItem | OnHand, Reserved counter, StockMovements (Reserved, Completed, Failed) | Quantity never negative; Reserved never above on hand | StockReserved, StockCompleted, StockReleased |
| Customers | Customer | Email and Address (value objects) | Email valid and unique | CustomerRegistered |
| Payments | Payment | Money, attempts | Amount equals the order total; one successful payment per order | PaymentCompleted, PaymentFailed |
| Notifications | Notification | Channel, recipient, attempts | Attempts not above the maximum (Options) | NotificationSent, NotificationFailed |
| Files | FileRecord | Metadata only (name, path, size, hash, owner link) | Pending becomes Active once saved and scanned (or Rejected); the unique name never changes | FileStored |

## Timeline: the main flow (place and pay an order)

| # | Actor or trigger | Command | Aggregate | Event | Context |
|---|------------------|---------|-----------|-------|---------|
| 1 | Visitor | Register | Customer (and Identity User) | CustomerRegistered | Customers, Identity |
| 2 | User | Log in | (Identity) | token issued (not a domain event) | Identity |
| 3 | Customer | Browse / search products (prefix) | read model | - | Catalog |
| 4 | Customer | Place order | Order | OrderPlaced | Orders |
| 5 | Policy: when an order is placed, reserve its stock | Reserve stock | StockItem | StockReserved | Catalog |
| 6 | *(hot spot H1)* | Confirm order | Order | OrderConfirmed | Orders |
| 7 | Policy: when an order is created, collect the payment | Process payment | Payment | PaymentCompleted or PaymentFailed | Payments |
| 8a | Policy: when payment completed | Mark order paid | Order | OrderPaid | Orders |
| 8b | Policy: when payment completed | Complete stock movement | StockItem | StockCompleted | Catalog |
| 9 | Policy: when payment failed *(hot spot H1)* | Cancel order / release stock | Order, StockItem | OrderCancelled, StockReleased | Orders, Catalog |
| 10 | Hangfire job, every 5 minutes | Mark expired reservations Failed (older than 15 minutes) | StockItem | StockReleased | Catalog |
| 11 | Payment after release *(hot spot H2)* | Compensate | Order / Payment | ? | Orders, Payments |
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

- **H1.** The order lifecycle: is `OrderConfirmed` raised when the stock is reserved (before payment),
  and does an expired reservation or a failed payment cancel the order automatically?
- **H2.** A payment that arrives after the reservation was marked Failed: a new reservation or a
  refund (left open in ADR-0001)?
- **H3.** The cart: does it exist in Phase 1, and which context owns it?
- **H4.** Customers: its own service in Phase 4, or inside Orders? How is a Customer created from an
  Identity user at registration?
- **H5.** Naming: the domain event is `OrderPlaced` but the integration event is `OrderCreated`.
  One name or two?
- **H6.** Which completes the stock movement on payment: Orders telling Catalog, or Catalog
  consuming `PaymentCompleted` directly?
- **H7.** Product creation, stock adjustment and stale-file marking have no domain event listed yet.
