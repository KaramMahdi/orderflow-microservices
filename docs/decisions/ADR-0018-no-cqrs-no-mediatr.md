# ADR-0018: No CQRS and no MediatR, Application Services

Date: 2026-11-24

Status: Accepted

Decided by: Karam

Risks: R-052

## Context

CQRS separates commands from queries with one handler per request, usually through a mediator. It
changes where the logic lives and adds a dependency.

## Options considered

- A. Keep Application Services, without CQRS.
- B. CQRS with MediatR (it moved to a commercial license in 2025; check its current terms).
- C. CQRS with a source-generated mediator.
- D. CQRS with handler interfaces and no mediator.
- E. Start without, and add it later.

## Decision

Option chosen: **A. Application Services, one method per use case. No CQRS, no MediatR.**

## Consequences

- Golden rules 1 and 2 stay as written: a controller calls one Application Service method, and all logic
  lives in Application Services, Domain Managers and the domain model.
- No mediator pipeline: cross-cutting concerns (validation, logging, unit of work) are handled in the
  Application Services and the shared middleware.
- Section 2.23 of the long plan is kept as a reference only.

## Revisit when

Karam decides to add CQRS later (option E).
