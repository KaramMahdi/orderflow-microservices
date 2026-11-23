# ADR-0007: Reads use IQueryable from repositories, executed through IAsyncQueryableExecuter

Date: 2026-11-24

Status: Accepted

Decided by: Karam

Risks: R-027

## Context

Exposing `IQueryable` from repositories can leak persistence into the Application layer: the
Application would need EF Core to run the async operators, and a query could be executed wrongly
(for example synchronously, or without paging).

## Options considered

- A. `GetQueryable()` on the repository plus an `IAsyncQueryableExecuter` abstraction. The
  Application stays free of EF Core.
- B. The Application references EF Core for the async operators (simpler, less pure).
- C. Specification objects passed to repository methods.

## Decision

Option chosen: **A, the executer abstraction.**

- Repositories expose `IQueryable` for reads. Application Services compose the query (filter, sort,
  page, project) and execute it through `IAsyncQueryableExecuter`.
- `IAsyncQueryableExecuter` is declared in Domain.Shared.Layer and implemented in
  EntityFrameworkCore.
- Reusable query rules can live in the Domain as expression-based specifications, applied with
  `Where` on the `IQueryable`, with no EF dependency.
- `DbContext` is injected nowhere except repositories (and the EF implementations next to them).

## Consequences

- The Application project never references EF Core. An architecture test checks it.
- Every list query is paged (ADR-0006) and names the index that serves it (golden rule 15).
- Tests: architecture test on project references; integration tests for the repositories.

## Revisit when

The executer abstraction becomes a real obstacle, for example a query that needs a provider-specific
operator.
