# ADR-0014: Product search with LIKE 'abc%' and an index

Date: 2026-11-24

Status: Accepted

Decided by: Karam

Risks: R-046

## Context

Customers search products by name. `LIKE 'abc%'` can use an index; `LIKE '%abc%'` cannot and scans
the table.

## Options considered

- A. Prefix search with `LIKE` and an index.
- B. SQL Server Full-Text Search (the default Linux image may not include it, so a custom image is needed).
- C. An external search engine container (more RAM).
- D. A normalized search column.

## Decision

Option chosen: **A, prefix search `LIKE 'abc%'` with an index on the product name.**

## Consequences

- Only "starts with" matches. A word in the middle of a name is not found.
- The index is recorded in [docs/indexes.md](../indexes.md).
- **Not decided here:** the collation of the name column and any normalization of Arabic letter
  variants, case and accents. The plan says to choose the collation deliberately.
- Tests: the search query uses the index (execution plan check), and a prefix search returns the
  expected products.

## Revisit when

Users need infix or full-text search, or Arabic search quality is not good enough (then option B, C or D).
