# Index catalog

**Status: starting design (Phase 0).** Source: section 2.16 of `docker-learning-project-plan.pdf`.
Every row is **Proposed** until Phase 1 confirms it against the real queries and records its name.

## Rules

- Start from the queries (use cases), not from the columns: every WHERE, JOIN (every foreign key),
  ORDER BY and uniqueness rule.
- Every index has written properties: unique or not, key columns and their order, ASC or DESC,
  INCLUDE columns, filter, and the query it serves.
- Every list or search query names the index that serves it (golden rule 15).
- Column order: equality columns first, then the range or sort column, matching the query's ORDER BY.
- INCLUDE only the columns a hot list query returns. Every included column widens the index (RAM).
- Filtered indexes for subsets (not deleted, pending, not null).
- Unique indexes enforce business rules (SKU, email, order number) and are a concurrency safety net.
- Do not index low-selectivity columns alone (Status, Source, flags). Use them in a composite or
  filtered index.
- Soft delete: unique and hot list indexes are filtered on `DeletedAt IS NULL`, so a deleted value can
  be reused.
- Text search: `LIKE 'abc%'` can use an index, `LIKE '%abc%'` cannot (ADR-0014).
- The Guid primary key is clustered by default (ADR-0010).
- Defined in Fluent API configuration classes in EntityFrameworkCore, so they flow into migrations
  and the snapshot. Names use the `IX_` (index) and `UX_` (unique) prefixes and live in an
  `IndexNames` constants class.
- Verify, do not guess: actual execution plans, Query Store, realistic data. Missing-index suggestions
  are hints only.
- Review the list each phase and drop unused indexes (`sys.dm_db_index_usage_stats`).

## Catalog

| # | Table | Key columns (order) | Properties | INCLUDE | Serves | Name | Status |
|---|-------|---------------------|-----------|---------|--------|------|--------|
| 1 | Users | NormalizedEmail | Unique | - | Login by email (ASP.NET Identity creates it) | Identity default | Proposed |
| 2 | Users | NormalizedUserName | Unique | - | Login by user name (ASP.NET Identity creates it) | Identity default | Proposed |
| 3 | RefreshTokens | TokenHash | Unique | - | Validate a refresh token | Phase 1 | Proposed |
| 4 | RefreshTokens | UserId, ExpiresAt | Filtered: not revoked | - | Active tokens of a user, cleanup | Phase 1 | Proposed |
| 5 | Products | Sku | Unique, filtered: not deleted | - | SKU lookup and uniqueness | Phase 1 | Proposed |
| 6 | Products | CategoryId, Name | Filtered: active and not deleted | Price, ImageFileId | Category listing sorted by name | Phase 1 | Proposed, see note A and B |
| 7 | Products | Name | Nonclustered | - | Prefix search `LIKE 'abc%'` (ADR-0014) | Phase 1 | Proposed |
| 8 | ProductPrices | ProductId, Currency | Unique, filtered: not deleted | Amount | The price of a product in a currency | Phase 1 | Proposed |
| 9 | StockItems | ProductId | Unique (one row per product) | - | Stock changes and the conditional reservation update (ADR-0001) | Phase 1 | Proposed, see note C |
| 10 | StockMovements | ProductId, Status | Nonclustered | Quantity | Sum of reserved quantity per product | Phase 1 | Proposed |
| 11 | StockMovements | Status, CreationTime | Filtered: Status = Reserved | ProductId, Quantity | The 5-minute job finds reservations older than 15 minutes | Phase 1 | Proposed |
| 12 | StockMovements | OrderId | Foreign key | Status, Quantity | Movements of an order | Phase 1 | Proposed |
| 13 | Orders | CustomerId, CreationTime DESC | Nonclustered | Status, Total, OrderNumber | "My orders" list, newest first | `IndexNames.Orders.ByCustomerCreated` | Proposed |
| 14 | Orders | OrderNumber | Unique | - | Lookup by business number, keyset export | `IndexNames.Orders.UniqueOrderNumber` | Proposed |
| 15 | Orders | Status, CreationTime | Filtered: open statuses | CustomerId | Operations queues (pending, processing) | Phase 1 | Proposed |
| 16 | Orders | Source, CreationTime | Only if reports need it | - | Reports by source (Source alone has too few values) | Phase 1 | Conditional |
| 17 | OrderItems | OrderId | Foreign key | ProductId, Quantity, UnitPrice | Load the items of an order | Phase 1 | Proposed |
| 18 | OrderItems | ProductId | Foreign key | - | Sales per product | Phase 1 | Proposed |
| 19 | Payments | OrderId, Status | Nonclustered | Amount | Payments of an order | Phase 1 | Proposed |
| 20 | Payments | ProviderReference | Unique, filtered: not null | - | Provider callbacks, duplicate protection | Phase 1 | Proposed |
| 21 | Notifications | RecipientId, CreationTime DESC | Nonclustered | Status | A user's notifications | Phase 3 | Proposed |
| 22 | Notifications | NextAttemptAt | Filtered: Status = Pending | - | Selecting notifications to retry | Phase 3 | Proposed |
| 23 | OutboxMessages | CreationTime | Filtered: ProcessedAt is null | - | Publisher reads unprocessed messages in order | Phase 3 | Conditional on R-012 |
| 24 | FileRecords | ModuleName, EntityName, EntityId | Nonclustered, filtered: not deleted | StorageName, ContentType, SizeBytes, Status | Files of a business record | Phase 1 | Proposed |
| 25 | FileRecords | StorageName | Unique | - | The unique name on disk is never duplicated | Phase 1 | Proposed |
| 26 | FileRecords | Status, CreationTime | Filtered: Status = Pending | - | The cleanup job finds stale Pending records | Phase 1 | Proposed |
| 27 | Translations | ModuleName, ListName, Name, Language | Unique, filtered: `DeletedAt IS NULL` | - | One text per key and language (section 2.24) | Phase 1 | Proposed |
| 28 | Translations | ModuleName, Language | Filtered: `DeletedAt IS NULL` | ListName, Name, Value | Load a whole module and language in one query (section 2.24) | Phase 1 | Proposed |

The "Name" column says where the name is set. Only rows 13 and 14 have a name in the plan; the
others get their `IX_` or `UX_` name in Phase 1.

## Notes to resolve in Phase 1

- **A. Category.** Row 6 uses `CategoryId`, but no Category exists in the domain model or the
  glossary yet. Either a Category is added to Catalog, or the row changes. Karam decides.
- **B. Price in INCLUDE.** Row 6 includes `Price`, but prices live in `ProductPrices`, one per currency
  (ADR-0003). The INCLUDE list must be revised.
- **C. Table name.** The plan's index table names this table `Stock`; the domain model calls the
  aggregate `StockItem`. This catalog uses `StockItems`.
- **D. Open statuses.** Row 15 filters on "open statuses (pending, processing)". The decided order
  lifecycle is Placed, Confirmed, Paid, Cancelled (ADR-0001), so the filter is aligned with it in Phase 1.
- Every row: confirm against the real query and its execution plan, then set the status to Confirmed.
