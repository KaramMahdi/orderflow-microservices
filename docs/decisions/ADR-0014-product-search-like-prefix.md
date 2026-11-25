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

Option chosen: **A with D: prefix search `LIKE 'abc%'` on a normalized search column, with an
index on that column.** D was added by Karam on 2026-11-25.

- A product keeps its `Name` as written and also stores `NormalizedName`.
- The same normalization runs on the product name when it is saved and on the search text before
  the query:
  - `أ`, `إ` and `آ` become `ا`.
  - `ة` becomes `ه`.
  - `ى` becomes `ي`.
  - Diacritics (tashkeel) and tatweel (`ـ`) are removed.
- The search query runs `LIKE 'abc%'` on `NormalizedName`, never on `Name`.

Why: the result does not depend on the database collation, and the normalization is covered by
unit tests.

## Consequences

- Only "starts with" matches. A word in the middle of a name is not found.
- One extra column and its index on the product table. The index is recorded in
  [docs/indexes.md](../indexes.md).
- `NormalizedName` is set by the Product aggregate whenever the name changes, so it never drifts.
- Tests: unit tests for every normalization rule; the search query uses the index (execution plan
  check); a search for `احمد` finds a product named `أحمد`.

## Revisit when

Users need infix or full-text search, or Arabic search quality is not good enough (then option B, C or D).
