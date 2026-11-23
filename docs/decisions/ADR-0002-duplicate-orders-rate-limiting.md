# ADR-0002: Duplicate orders are limited by rate limiting

Date: 2026-11-24

Status: Accepted

Decided by: Karam

Risks: R-002

## Context

A double click or a client retry can send the same create-order request twice and create a
duplicate order.

## Options considered

- A. Idempotency key header with a unique index.
- B. Disable the button only (weak, the client can still retry).
- C. Accept duplicates.
- D. Rate limiting on order creation.

## Decision

Option chosen: **D, rate limiting.** One create-order request per user per minute (tunable).

- The built-in ASP.NET Core rate limiter, one policy per endpoint group. The partition is the user id
  from the token, falling back to the IP address for anonymous endpoints.
- Policy names are constants in Domain.Shared.Layer (`RateLimitPolicies.CreateOrder`). The permit
  limit and the window come from Options.
- A rejected request answers 429 with an `ErrorDto` (`ErrorCode.TooManyRequests`, key
  `Common.TooManyRequests`) in the request language, plus a `Retry-After` header. It is written by
  the shared handler in Shared.AspNetCore.
- The controller only gets the `[EnableRateLimiting]` attribute. It still contains no logic.

Why: simple, no extra table, and it also protects the endpoint.

## Consequences

- **Limits accepted:** rate limiting is not idempotency. A legitimate second order inside the window
  is rejected, and a retry after the window creates a duplicate.
- The built-in limiter counts per process. From Phase 4 the gateway is the single entry point and
  applies the order limit. Several gateway instances would need a Redis-backed limiter.
- Performance tests must not end up measuring 429 answers (R-071, open).
- Logging: a Warning with the user id, the policy and the request source.
- Tests: a second create-order call inside the window returns 429 with an `ErrorDto`; a call after the
  window succeeds.

## Revisit when

Duplicates start to matter: an idempotency key can then be added without changing the rest. Also
Phase 4, when the gateway takes over the limit.
