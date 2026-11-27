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
- **Precision and scale (decided by Karam, 2026-11-25):** every money amount column is
  `decimal(18,2)`: 18 digits in total, 2 of them after the decimal point.
- **More than 2 decimal places (decided by Karam, 2026-11-27): rejected, never rounded.**
  `Money.Create` throws an `ErrorException` with `ErrorCode.ValidationFailed` and
  `Common.MoneyAmountTooPrecise`.
- **Currencies with 3 minor units (decided by Karam, 2026-11-27): not supported for now.** Every
  currency uses 2 decimal places, matching `decimal(18,2)`. Supporting KWD, BHD, JOD or OMR needs a new
  decision (a wider scale or a scale per currency).
- **Currency code (decided by Karam, 2026-11-27): format only.** The domain accepts three letters
  (ISO 4217) and stores them in upper case. A list of allowed currencies, if one is needed, is checked
  in the Application layer from Options, not in the domain.
- Tests: domain unit tests for `Money` (equality, arithmetic, currency mismatch) and for the Order
  currency invariant.

## Revisit when

A real need for conversion appears (option B or C).
