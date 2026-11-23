# ADR-0023: Phase order: security and performance tests before the Vue.js frontend

Date: 2026-11-24

Status: Accepted

Decided by: Karam

Risks: none (a planning decision)

## Context

The backend is built in Phases 1 to 5. The frontend could come right after it, or after the system has
been tested for security and performance.

## Options considered

- A. The Vue.js frontend right after Phase 5.
- B. Security tests (Phase 6) and performance tests (Phase 7) first, then the Vue.js frontend (Phase 8).

## Decision

Option chosen: **B.**

1. Modular monolith, 2. Redis, 3. RabbitMQ, 4. Services, 5. Production and Seq,
6. Security tests, 7. Performance tests, 8. Vue.js frontend (later, not in scope now).

Both test phases run on the Compose stack.

## Consequences

- The frontend is built on a backend that has already been attacked and measured.
- The README of `main` describes Phase 7 until Phase 8 starts.
- RabbitMQ stays in Phase 3, before the split into services.
- The backend prepares for Phase 8 from the start: an OpenAPI document, CORS from Options, every
  error as an `ErrorDto`, the CorrelationId in the response header.

## Revisit when

The frontend is needed earlier.
