# ADR-0004: The server's local time

Date: 2026-11-24

Status: Accepted

Decided by: Karam

Risks: R-004

## Context

Every timestamp (audit columns, reservations, jobs) needs one clear time base. Linux containers run
in UTC by default.

## Options considered

- A. The server's local time (`DateTime.Now`).
- B. UTC with `DateTimeOffset`.

## Decision

Option chosen: **A, the server's local time.**

- Time is read through `TimeProvider` (the server's local time, testable).
- Every container sets the same `TZ`, from `.env`.

## Consequences

- The image must contain `tzdata` (Alpine and chiseled images need it added).
- Hangfire uses the local zone too.
- Daylight saving repeats one hour and skips another. The 15-minute reservation cut-off can be an
  hour early or late on those days (ADR-0001).
- Times from servers in different zones cannot be compared directly.
- All of the above is accepted.

## Revisit when

Servers in different time zones must be compared, or daylight-saving effects cause a real problem.
