# ADR-0009: The Hangfire dashboard is reachable only from localhost or inside the server

Date: 2026-11-24

Status: Accepted

Decided by: Karam

Risks: R-031

## Context

The Hangfire dashboard shows and controls every job. Exposed publicly, it is a security hole. Job
storage also grows over time.

## Options considered

- A. The dashboard is reachable only from inside the server or from localhost.
- B. A dashboard protected by a cookie.
- C. Access through a browser extension.

## Decision

Option chosen: **A.**

- The dashboard listens on its own internal port, bound to `127.0.0.1`. Normal users never see it.
- Job history retention (in hours or days) comes from Options.
- Each service has its own Hangfire schema (from Phase 4).
- The deletion of Hangfire's own data by its retention is the accepted exception to the
  no-delete rule (ADR-0005).

## Consequences

- The dashboard port is never published by Compose on a public interface.
- Phase 6 checks it: the dashboard must be unreachable from the public port.
- Tests: an exposure check in Phase 6 security tests.

## Revisit when

Operations staff need the dashboard from outside the server.
