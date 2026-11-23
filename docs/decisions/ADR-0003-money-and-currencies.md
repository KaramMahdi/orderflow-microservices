# ADR-0003: Money as decimal, several currencies, one currency per order

Date: 2026-11-24

Status: Accepted

Decided by: Karam

Risks: R-003, R-063

## Context

Floating-point money causes rounding errors. The system sells in several currencies, so it must be
clear how prices are stored and whether an order can mix or convert currencies.

## Options considered

R-003 (money type): the register records only the chosen approach, decimal with fixed precision
inside a `Money` value object.

R-063 (several currencies):

- A. Each order has one currency and no conversion. Prices are stored per currency.
- B. A base currency with an exchange-rate table, conversion at order time, the rate saved on the order.
- C. Conversion only for display.

## Decision

Option chosen: **decimal in a `Money` value object (amount and currency code), and R-063 A.**

- Each order has exactly one currency. Every item of the order is in that currency. No conversion.
- A product has one price per currency (`ProductPrice`, unique on product and currency).
- `Money` is immutable, compared by value and created through a validated factory method.

Why: exact arithmetic, and no exchange-rate data to maintain.

## Consequences

- A product without a price in the order's currency cannot be added to that order.
- Totals are summed in one currency only, which keeps the Order invariant simple
  ("all items in the order's one currency").
- **Not decided here:** the exact precision and scale of the money columns. They are set and
  recorded in Phase 1.
- Tests: domain unit tests for `Money` (equality, arithmetic, currency mismatch) and for the Order
  currency invariant.

## Revisit when

A real need for conversion appears (option B or C).
