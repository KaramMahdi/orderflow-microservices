# AGENTS.md

Instructions for AI tools working in this repository. Read this file first, every session. Detail lives
in `docs/`; this file links to it instead of copying it.

## Project summary and current phase

OrderFlow is an order-management reference system built with full DDD on .NET 10, SQL Server and
Docker, delivered phase by phase (see [README.md](README.md)).

- **Current phase: 1 (modular monolith).** Started by Karam. Branch `phase-1-modular-monolith`.
  Phase 0 (foundation: architecture documents and this file) is done.
- **Next phase: 2 (Redis).** Not started. It starts only when Karam says so.
- Bounded contexts: Orders, Catalog, Customers, Payments, Notifications, Identity, Files
  ([context map](docs/context-map.md), [glossary](docs/glossary.md)).

## The 20 golden rules

1. Controllers contain no logic. They bind a DTO, call one Application Service method and return the result.
2. All logic lives in Application Services, Domain Managers and the domain model.
3. All database access goes through repositories. DbContext is injected nowhere else. Reads use IQueryable, executed through IAsyncQueryableExecuter.
4. Everything shared between projects lives in Domain.Shared.Layer.
5. Every DTO lives in its proper place: Application.Contracts for a context, Domain.Shared.Layer for generic ones. The Domain never sees a DTO.
6. Mapping uses Mapster, only inside Application Services.
7. NuGet versions are declared once, in Directory.Packages.props.
8. No hard-coded values: Options, constants, enums, and the Translations table for messages.
9. Log generously and structurally ([LoggerMessage], ILogger<T>). Never log secrets.
10. Memory-conscious and best practice by default. Measure before micro-optimizing.
11. No phase starts before its risks have a written decision (an ADR) made by me.
12. All background work runs as Hangfire jobs, inside the Host. Jobs are thin and idempotent.
13. Every identifier is a Guid.
14. No file content in the database. Files live on the server under a unique name, linked to their record.
15. Index the key columns and define their properties. Every list or search query names its index.
16. Every request has a source (Web, Mobile, Api) taken from the client_id claim in the JWT.
17. Swagger is available in every environment, with a Bearer Authorize button.
18. Every phase has its own branch and its own README. Pull Requests are squashed into main.
19. Six audit columns on every table, defined once in Entity: CreatorId, CreationTime, LastModifierId, LastModificationTime, DeletedBy, DeletedAt. All nullable. Soft delete only.
20. No try/catch unless something is actually handled. One shared handler turns every error into an ErrorDto (a code and a message from the Translations table).

"Me" in rule 11 is Karam, the owner of this repository.

## Solution structure

One solution: `OrderFlow.sln`.

Common projects:

| Project | Holds |
|---------|-------|
| `Domain.Shared.Layer` | Base types (Entity with audit columns, AggregateRoot, ValueObject), abstractions (repositories, unit of work, IAsyncQueryableExecuter, IRequestContext, IFileStorage, ICacheService, IMessagePublisher, IFileOwnerAccessChecker), shared DTOs, integration events, constants, ErrorCode, ErrorDto, ErrorException, MessageKey |
| `Shared.AspNetCore` | Shared middleware for every host: error handling, CorrelationId, request context, language |
| `Shared.Infrastructure` | Redis, RabbitMQ and file storage adapters, written once |

Per context, the same set: `.Domain.Shared`, `.Domain`, `.Application.Contracts`, `.Application`,
`.EntityFrameworkCore`, `.HttpApi`, `.HttpApi.Client`, `.HttpApi.Host`, `.DbMigrator`.

- Phase 1 uses the prefix `OrderFlow.*` for one set, with the contexts as modules (folders).
- Phase 4 splits it into Identity, Catalog, Orders, Payments, Notifications and Files, plus `Gateway`.
- Payments also has `Payments.Infrastructure` (the payment provider adapter).
- `OrderFlow.Domain.Shared` (per context) is not `Domain.Shared.Layer` (common). Do not mix them.

Project reference rules (checked by architecture tests):

- `HttpApi` and `HttpApi.Client` reference only `Application.Contracts`.
- `Domain` references only the two Domain.Shared projects (its context's `.Domain.Shared` and `Domain.Shared.Layer`).
- `EntityFrameworkCore` references `Domain`, never `Application`.
- `HttpApi.Host` is the only project that knows everything.
- `Domain`, `Application`, `Application.Contracts` and `HttpApi.Client` never reference the Redis or RabbitMQ packages.

## Naming and code conventions

Enforced by [.editorconfig](.editorconfig) where possible.

- File-scoped namespaces. Nullable reference types enabled (set in `Directory.Build.props`, Phase 1).
- PascalCase for types, methods and properties. camelCase for locals and parameters.
  `_camelCase` for private fields. `I` prefix for interfaces. `Async` suffix for async methods.
- Suffixes: `AppService` (Application Service), `Manager` (Domain Manager), `Repository`,
  `Dto`, `IntegrationEvent`, `Options` (configuration classes, each with a `SectionName` constant).
- Constants classes instead of literals: `MessageKeys`, `Modules`, `IndexNames`, `RateLimitPolicies`,
  `ApiHeaders`, `AuditColumns`, role, claim and policy names (all in Domain.Shared.Layer or the
  context's `.Domain.Shared`).
- Enums stored in the database have explicit numbers that are never reordered.
- Index names use `IX_` and `UX_` prefixes, live in `IndexNames`, and are recorded in [docs/indexes.md](docs/indexes.md).
- Code and names follow the [glossary](docs/glossary.md). When the language changes, the code changes.
- Domain: small aggregates, invariants in the root, no public setters, business-named methods,
  other aggregates referenced by Guid only, one repository per aggregate root.
- Errors: throw `ErrorException(ErrorCode, MessageKey, args)`. The Domain never sets an HTTP status
  or writes a message text.
- Read the time through `TimeProvider` (server local time, ADR-0004), so it stays testable.

## Decisions summary

All Accepted, decided by Karam. Full register: [docs/risks.md](docs/risks.md).

- [ADR-0001](docs/decisions/ADR-0001-stock-reservation.md): stock reserved as StockMovements, atomic by a Reserved counter; job every 5 min releases reservations older than 15 min (R-001, R-001b).
- [ADR-0002](docs/decisions/ADR-0002-duplicate-orders-rate-limiting.md): duplicate orders limited by rate limiting, one create-order per user per minute (R-002).
- [ADR-0003](docs/decisions/ADR-0003-money-and-currencies.md): decimal Money, several currencies, one currency per order, one price per currency (R-003, R-063).
- [ADR-0004](docs/decisions/ADR-0004-server-local-time.md): server local time, same TZ in every container (R-004).
- [ADR-0005](docs/decisions/ADR-0005-no-deletion-soft-delete-only.md): nothing deleted, soft delete only, no ExecuteDelete (R-005, R-050, R-031 exception).
- [ADR-0006](docs/decisions/ADR-0006-paging.md): paging default 10, maximum 1000, from Options (R-006).
- [ADR-0007](docs/decisions/ADR-0007-iqueryable-and-executer.md): IQueryable from repositories, executed through IAsyncQueryableExecuter (R-027).
- [ADR-0008](docs/decisions/ADR-0008-hangfire-in-host-and-job-rules.md): Hangfire inside the Host; no overlap, never killed, idempotent, limited attempts (R-029, R-030).
- [ADR-0009](docs/decisions/ADR-0009-hangfire-dashboard-localhost-only.md): Hangfire dashboard only on 127.0.0.1, own internal port (R-031).
- [ADR-0010](docs/decisions/ADR-0010-guid-identifiers.md): Guid always, CreateVersion7, one-million-row test (R-038, R-039).
- [ADR-0011](docs/decisions/ADR-0011-file-storage-naming-and-access.md): files on a volume, Files service, Guid names, module/date folders, owner access check (R-040, R-041, R-061, R-062).
- [ADR-0012](docs/decisions/ADR-0012-upload-safety-and-consistency.md): size limit, signature allow-list, ClamAV; Pending, then Active (R-042, R-043).
- [ADR-0013](docs/decisions/ADR-0013-request-source-from-client-id.md): request source from the client_id claim; anonymous is Unknown (R-045).
- [ADR-0014](docs/decisions/ADR-0014-product-search-like-prefix.md): product search with LIKE 'abc%' and an index (R-046).
- [ADR-0015](docs/decisions/ADR-0015-swagger-in-every-environment.md): Swagger in every environment; built-in OpenAPI document with Swagger UI (R-048).
- [ADR-0016](docs/decisions/ADR-0016-nullable-audit-columns.md): all six audit columns nullable (R-049).
- [ADR-0017](docs/decisions/ADR-0017-shared-projects.md): Shared.Infrastructure and Shared.AspNetCore (R-051, R-055).
- [ADR-0018](docs/decisions/ADR-0018-no-cqrs-no-mediatr.md): no CQRS, no MediatR; Application Services (R-052).
- [ADR-0019](docs/decisions/ADR-0019-git-pull-requests-squash.md): phase branches, squashed Pull Requests into main, a tag per phase (R-053).
- [ADR-0020](docs/decisions/ADR-0020-one-solution-and-payments-infrastructure.md): one big solution; Payments.Infrastructure (R-054, R-060).
- [ADR-0021](docs/decisions/ADR-0021-errors-translations-and-errordto.md): Translations table per service; ErrorDto code + message; memory or empty fallback (R-056, R-057, R-058).
- [ADR-0022](docs/decisions/ADR-0022-naming-and-solution-layout.md): Domain.Shared.Layer and the DealerOperations-style split; no Hangfire project.
- [ADR-0023](docs/decisions/ADR-0023-phase-order.md): security (6) and performance (7) tests before the Vue.js frontend (8).

Open points inside decided ADRs are listed at the end of [docs/risks.md](docs/risks.md).

## Workflow

Phases: 1 modular monolith, 2 Redis, 3 RabbitMQ, 4 services, 5 production and Seq, 6 security tests,
7 performance tests, 8 Vue.js frontend (later).

The loop for every phase:

1. **Decide:** every risk of the phase has an Accepted ADR in `docs/decisions/` (made by Karam).
2. **Branch:** `phase-N-<name>` from `main` (for example `phase-1-modular-monolith`).
3. **Build:** the milestones in order; each milestone ends with its tests.
4. **Document:** the phase README, the ADRs, `docs/indexes.md`, the glossary, the context map.
5. **Prove it:** clone the branch into a new folder and follow the README literally. Fix every failing step.
6. **Merge:** one Pull Request, reviewed against the checklist, squashed into `main`.
7. **Tag:** `v1-monolith`, `v2-redis`, `v3-rabbitmq`, `v4-services`, `v5-production`, `v6-security`, `v7-performance`.

README per phase: each phase branch has its own root README (what the phase adds, architecture
diagram in Mermaid, how to run it, URLs, test users from `.env`, Swagger steps, links to the phase's
ADRs, tests, Docker concepts, troubleshooting, links to the previous and next phase). No secrets in a
README; point to `.env.example`.

Pull Request checklist:

- [ ] The golden rules hold (architecture tests green).
- [ ] All tests green: domain unit tests, integration tests on a real SQL Server container, architecture tests.
- [ ] `docker compose up` works from a clean clone; memory checked with `docker stats`.
- [ ] The phase README is written, tested from a clean clone, and links the phase's ADRs.
- [ ] The ADRs, `docs/indexes.md`, the glossary and the context map are updated.
- [ ] No secrets in the repository (only `.env.example`).

## How to work with Karam

- **Never decide an Open risk yourself.** List the options and ask Karam.
- **Never start a phase before its risks have an Accepted ADR.**
- **Do not add features outside the plan.** If something seems missing, ask.
- **Do not invent decisions.** If it is not in an ADR or the plans, mark it Open and ask.
- If the plans and a request disagree, list the disagreement and ask; do not choose silently.
- Reply to Karam in Arabic. Write every file in the repository in English.
- Push, add remotes, create branches or create tags only when Karam asks. Never force push.

## Session rules

These rules apply to every working session. "Me" and "my" mean Karam.

- Reply to me in Arabic. Write every file in English.
- Do only the current prompt. Do not add features outside the plan.
- Never decide an open risk yourself. If something is Open or not covered by AGENTS.md, docs/risks.md or the ADRs, stop and ask me.
- If AGENTS.md, the ADRs and the PDFs disagree, list the disagreement and ask me.
- Do not guess package versions or library APIs. Check the current documentation. If you cannot check, say so and leave a TODO.
- Keep every golden rule. Nothing hard-coded: Options, constants, enums.
- Write the tests together with the code of each milestone.
- NuGet versions only in Directory.Packages.props.
- Small commits with clear messages. Never push. Never force push.
- Finish with the real output of dotnet build, dotnet test, and docker compose up --build when it applies. No summaries of output.
- Every prompt ends with a report in Arabic: what you built, what you did not do, every deviation from the plan, every assumption, and what you need from me. Then stop.
- A prompts (Build) stop for my review and do not commit. B prompts (Prove) end with a commit.
- Branch: phase-1-modular-monolith. Plan: docs/orderflow-phases-plan.pdf. Reference: docs/docker-learning-project-plan.pdf (read only the section you need).

## Commands

To be filled in Phase 1 (build, test, run with Docker Compose, add a migration).
