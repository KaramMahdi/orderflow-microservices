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

Why: no read-then-write gap, no retries, little memory, and the model matches real life (held items).

## Consequences

- The counter is a second source of truth. A test (and optionally a periodic check) compares
  `Reserved` with the sum of the Reserved movements.
- The conditional update uses `ExecuteUpdateAsync`, which skips the audit interceptor, so a helper
  sets `LastModifierId` and `LastModificationTime` there (see ADR-0005).
- The 15-minute cut-off uses the server clock, so a daylight-saving change can release reservations
  an hour early or late (accepted in ADR-0004).
- Race to watch: a payment can arrive just as the job marks the movement Failed. The order then
  needs a compensation. **Which compensation (a new reservation or a refund) is not decided yet.**
- **Not decided here:** the CHECK constraint `0 <= Reserved <= OnHand` (R-001 option G) is noted as
  a cheap safety net in the plan, but it is not decided.
- Tests: the last-unit concurrency test on a real SQL Server container (N parallel CreateOrder calls
  with `Task.WhenAll`, exactly one succeeds, stock ends at 0), and the counter-equals-movements test.

## Revisit when

Phase 4, when the stock moves to the Catalog service and orders to the Orders service, so one
database transaction can no longer cover both (R-016).
