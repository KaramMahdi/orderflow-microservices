# ADR-0016: All six audit columns are nullable

Date: 2026-11-24

Status: Accepted

Decided by: Karam

Risks: R-049

## Context

Every table has six audit columns, defined once in the base `Entity`: `CreatorId`, `CreationTime`,
`LastModifierId`, `LastModificationTime`, `DeletedBy`, `DeletedAt` (golden rule 19). It must be
decided whether they accept null, and who acts when there is no user (jobs, seeding).

## Options considered

- `CreationTime` nullable, or NOT NULL with a default (queries sort by it).
- System work records null, or a well-known system Guid constant.
- Background jobs act as the system, or as the user who enqueued them.

## Decision

Option chosen: **all six columns accept null.**

- The application always fills them through one audit interceptor, so a null means a record added by
  hand, outside the application.
- With no user (a Hangfire job, a seed) the user columns stay null.

## Consequences

- The setters are private; only the interceptor writes them. Input DTOs never contain them.
- Time comes from `TimeProvider` (server local time, ADR-0004). The user comes from `IRequestContext`.
- Queries that sort by `CreationTime` put rows with null last or ignore them.
- No foreign keys on `CreatorId`, `LastModifierId` and `DeletedBy`: users live in Identity, and the
  audit trail must survive changes to users.
- Exceptions (tables owned by a framework): the Hangfire schema, `__EFMigrationsHistory`, and the
  ASP.NET Identity link tables unless customized. `ApplicationUser` implements the same
  `IFullAudited` interface.
- Tests: the interceptor integration test (insert, update, soft delete), and a model test with an
  explicit allow-list that fails if any other table lacks the six columns.

## Revisit when

Reports need a guaranteed creation time, or jobs must be attributed to a user.
