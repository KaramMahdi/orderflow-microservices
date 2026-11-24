# ADR-0001: Stock reservation with StockMovements and a Reserved counter

Date: 2026-11-24

Status: Accepted

Decided by: Karam

Risks: R-001, R-001b

## Context

A product has 1 unit in stock and two customers order it in the same second. With read-then-write
code both orders read the stock before either writes it, and both are confirmed: two orders, one
unit. Isolation levels alone do not solve it. The reservation model itself (R-001b) also needs to be
atomic, and an order with several products must reserve all of them or none.

## Options considered

R-001 (how to stop overselling):

- A. Atomic conditional update: one statement decreases stock only where it is enough, then checks the rows affected.
- B. Optimistic concurrency with a rowversion: a concurrent change throws, then retry or reject.
- C. Pessimistic lock (UPDLOCK, ROWLOCK) inside a transaction.
- D. Stock reservation for N minutes: confirm on payment, release on expiry. Still needs A, B or C.
- E. One queue per product through RabbitMQ (single active consumer).
- F. Distributed lock in Redis. Not safe alone.
- G. CHECK constraint (stock >= 0) as a last line of defense.

R-001b (how the reservation is protected): A. a Reserved counter updated by one conditional
statement; B. a rowversion with retry; C. a row lock.

## Decision

Option chosen: **R-001 D, made atomic by R-001b A.**

- Placing an order adds a `StockMovement` with status **Reserved** (quantity, order id).
- Payment completed: the movement becomes **Completed** (the stock really leaves).
- Not paid: a Hangfire job runs every 5 minutes and marks the movements still Reserved after
  15 minutes as **Failed** (the reservation is released). Both values come from Options, so a
  reservation really lives between 15 and 20 minutes.
- `StockItem` has a `Reserved` counter. Creating a reservation is one conditional update: increase
  `Reserved` by the quantity only if `OnHand - Reserved` is enough. The Reserved movement is
  added in the same transaction. If the update touches no row, the stock is not enough
  (`ErrorException` with `ErrorCode.Conflict`).
- The counter and the movements change together: a new Reserved movement adds to the counter;
  Completed lowers both `OnHand` and `Reserved`; Failed lowers `Reserved`.
- Available quantity = `OnHand - Reserved`. Reserved movements count as taken, Failed ones do not.
- An order with several products reserves them all in one transaction, in `ProductId` order. If one
  product fails, everything rolls back.
- Completing a movement updates only rows that are still Reserved.

CHECK constraint (R-001 option G, decided by Karam on 2026-11-24): **added.**

- `StockItems` has `CHECK (Reserved >= 0 AND Reserved <= OnHand)`. This also guarantees
  `OnHand >= 0`.
- It is a last safety net. The domain still enforces the StockItem invariant first ("Reserved never
  above on hand"): lowering `OnHand` below `Reserved` throws an `ErrorException` before saving.
- It is defined in Fluent API (`ToTable(t => t.HasCheckConstraint(...))`), its name lives in a
  constants class, and it flows into the migration.
- A violation (SQL Server error 547 on this constraint) is mapped by the EF exception mapper to
  **409 Conflict** (`ErrorCode.Conflict`, `Common.Conflict`), the same way as a unique-index
  violation. It is logged as a Warning with the CorrelationId (section 2.24).

Order lifecycle (decided by Karam on 2026-11-24):

- **Confirmed means the stock is reserved.** `OrderConfirmed` is raised when the reservation of every
  product of the order succeeds.
- The path is: Placed, then Confirmed (stock held, waiting for payment), then Paid or Cancelled.
- In Phase 1 the reservation runs in the same transaction as placing the order, so `OrderPlaced` and
  `OrderConfirmed` happen together. If the stock is not enough, the order is not created (409).
- From Phase 4 the reservation is asynchronous (Orders waits for Catalog's `StockReserved`), so an
  order can stay Placed for a while. What happens to a Placed order when Catalog has no stock is
  decided with R-016.
- The exact status enum names and numbers are set in Phase 1, following these states.

Order status when the order is not paid (decided by Karam on 2026-11-24):

- A failed payment does **not** cancel the order. The order keeps waiting for payment while its
  reservation lives.
- An expired reservation (the job marks the movement Failed) **cancels the order automatically**
  (OrderCancelled).

Late payment compensation (decided by Karam on 2026-11-24): **always refund.**

- A late payment is a payment that arrives after its reservation expired: completing the movement
  touches no row because the job already marked it Failed, and the order is already Cancelled.
- A Cancelled order is never reopened. The late payment is always refunded, even if the stock is
  available again. The customer places a new order.
- No new reservation is attempted.
- The handling is idempotent: a payment event delivered twice must not refund twice.
- It is logged as a Warning with the order id, the payment id and the CorrelationId.

Why: no read-then-write gap, no retries, little memory, and the model matches real life (held items).

## Consequences

- The counter is a second source of truth. A test (and optionally a periodic check) compares
  `Reserved` with the sum of the Reserved movements.
- The conditional update uses `ExecuteUpdateAsync`, which skips the audit interceptor, so a helper
  sets `LastModifierId` and `LastModificationTime` there (see ADR-0005).
- The 15-minute cut-off uses the server clock, so a daylight-saving change can release reservations
  an hour early or late (accepted in ADR-0004).
- Race to watch: a payment can arrive just as the job marks the movement Failed. It is handled by
  the late payment compensation above.
- Payments gets a refund operation (decided by Karam on 2026-11-24). In Phase 1 it is simulated,
  through the provider adapter in Payments.Infrastructure.
- Cancelled is a final status for an order: no transition leaves it. The order stays Cancelled after
  the refund.
- The customer loses the order even when the stock is available again. Accepted for simpler rules.
- Tests: a failed payment leaves the order waiting; an expired reservation cancels the order; a late
  payment is refunded whether or not stock is available, and the order stays Cancelled; the same late
  payment delivered twice is refunded only once.
- With the CHECK constraint, a bug that would store an impossible stock (for example a job releasing
  the same movement twice) fails loudly instead of saving silently. Because it answers 409, such a
  bug looks like a normal conflict to the client; the logs must be watched for it.
- Tests: the last-unit concurrency test on a real SQL Server container (N parallel CreateOrder calls
  with `Task.WhenAll`, exactly one succeeds, stock ends at 0), and the counter-equals-movements test. For the constraint: an update that
  would make `Reserved` negative or above `OnHand` is rejected by SQL Server and returns 409 with an
  `ErrorDto`; lowering `OnHand` below `Reserved` is rejected by the domain.

## Revisit when

Phase 4, when the stock moves to the Catalog service and orders to the Orders service, so one
database transaction can no longer cover both (R-016).
