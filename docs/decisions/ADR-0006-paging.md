# ADR-0006: Paging, default 10 and maximum 1000

Date: 2026-11-24

Status: Accepted

Decided by: Karam

Risks: R-006

## Context

An unbounded list query loads every row into memory and can take the API down.

## Options considered

- Offset pagination or cursor (keyset) pagination.
- The default and maximum page size, from Options.

## Decision

Option chosen: **every list query is paged. Default page size 10, maximum 1000, both from Options.**

- **Offset pagination (decided by Karam, 2026-11-25):** a page skips the rows of the earlier pages
  and takes the page size. With a page size of 10: skip 0 and take 10, then skip 10 and take 10,
  then skip 20 and take 10, and so on.
- A request above the maximum is limited to the maximum (or rejected), never executed unbounded.
- Paging uses the generic `PagedRequestDto` and `PagedResultDto<T>` from Domain.Shared.Layer.
- The `ORDER BY` columns of a list query match an index (see [docs/indexes.md](../indexes.md)).

## Consequences

- No unbounded collections reach the API or the memory.
- **Still open:** whether a page size above the maximum is clamped or rejected. The index catalog
  also mentions a keyset export on `OrderNumber`; it is not the general list approach.
- Tests: a request without a size gets 10 items; a request above 1000 never returns more than 1000.

## Revisit when

Phase 7 performance tests show that deep offset pages are slow.
