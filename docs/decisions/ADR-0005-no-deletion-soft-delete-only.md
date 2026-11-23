# ADR-0005: Nothing is deleted for now, soft delete only

Date: 2026-11-24

Status: Accepted

Decided by: Karam

Risks: R-005, R-050, and the R-031 exception

## Context

The `DeletedBy` and `DeletedAt` audit columns imply soft delete. Still open were: which data may be
hard deleted, when old soft-deleted rows are purged, and how bulk operations (`ExecuteUpdate`,
`ExecuteDelete`) behave, because they skip the audit interceptor.

## Options considered

R-005: decide which data may be hard deleted; purge old soft-deleted rows after a period; SQL Server
temporal tables for full history.

R-050: a purge job after a retention period; keep forever; `ExecuteUpdate` and `ExecuteDelete` set the
audit columns through a helper, or are not used on audited tables.

## Decision

Option chosen: **nothing is deleted for now. Soft delete only.**

- The audit interceptor turns every delete into a soft delete (`DeletedBy`, `DeletedAt`).
- A global query filter hides rows where `DeletedAt` is not null. Only admin or restore use cases
  ignore it.
- `ExecuteDelete` is not used.
- `ExecuteUpdateAsync` is used where needed (the stock reservation, ADR-0001) and sets the audit
  columns through a helper.
- Files: the record is soft deleted and the content stays on disk.
- Exception, accepted: Hangfire, Seq and Redis expire their own data by design.

## Consequences

- The database and the files disk only grow. The volume size and the backups (R-044, open) must
  plan for it.
- Unique indexes are filtered on `DeletedAt IS NULL`, so a deleted value (for example a SKU) can be
  reused.
- The audit columns show who and when, last time only. They are not a full history.
- Tests: the soft-delete integration test (a delete sets the columns and the row is hidden), and a test
  that the reservation helper sets the audit columns.

## Revisit when

Karam finds a technique for old data (purge, archive or temporal tables). A new ADR supersedes
this one.
