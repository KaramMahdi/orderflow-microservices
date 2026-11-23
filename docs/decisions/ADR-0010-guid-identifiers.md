# ADR-0010: Every identifier is a Guid, generated with CreateVersion7

Date: 2026-11-24

Status: Accepted

Decided by: Karam

Risks: R-038, R-039

## Context

Every identifier is a Guid (golden rule 13): entity keys, foreign keys, event ids, message ids, file
ids. Random Guids fragment a clustered index. Strongly typed ids add safety but also converters and
code.

## Options considered

R-038 (generation and index cost):

- A. `Guid.CreateVersion7()` created in the domain.
- B. The EF Core sequential Guid generator.
- C. Random `Guid.NewGuid()`.
- D. Guid as a nonclustered primary key, with the clustered index on a narrow column.

R-039 (plain or typed):

- A. Raw `Guid` everywhere.
- B. A typed id per aggregate (for example `OrderId` as a readonly record struct) with converters.
- C. Typed ids for aggregate roots only.

## Decision

Option chosen: **R-038 A and R-039 A.** Raw `Guid` always, generated with `Guid.CreateVersion7()`
in the domain.

## Consequences

- The Guid primary key is clustered by default, so the generation strategy matters for the index
  (see [docs/indexes.md](../indexes.md)).
- SQL Server orders `uniqueidentifier` values by a different byte order than the version 7 layout, so
  the real fragmentation must be measured, not assumed.
- Test: a one-million-row test with `CreateVersion7` on a real SQL Server container verifies the
  insert behavior and fragmentation.
- Without typed ids, the compiler does not stop an `OrderId` being passed where a `ProductId` is
  expected. Parameter names and tests must cover it.

## Revisit when

The one-million-row test shows unacceptable fragmentation (then reconsider option B or D of R-038).
