# Risk register

Every known risk gets a written decision made by Karam before its phase starts. Accepting a risk is a
valid decision, as long as it is written down.

- **Phase gate:** a phase starts only when all its risks have an ADR with status Accepted.
- Each decision names a test that proves it works.
- A decision can be revisited: a new ADR supersedes the old one.
- **Open** rows list options only. They are starting points, not choices. AI tools never decide an
  Open risk (see [AGENTS.md](../AGENTS.md)).

Source: section 2.11 of `docker-learning-project-plan.pdf` and `orderflow-phases-plan.pdf`.

## Summary

| Phase | Decided | Open |
|-------|---------|------|
| Before Phase 1 | R-001, R-001b, R-002 to R-006, R-027, R-029 to R-031, R-038 to R-043, R-045, R-046, R-048 to R-053, R-055 to R-058, R-060, R-062, R-063 | none |
| Before Phase 2 | - | R-007, R-008, R-009, R-010, R-059 |
| Before Phase 3 | - | R-011, R-012, R-013, R-014, R-015, R-028 |
| Before Phase 4 | R-054, R-061 | R-016, R-017, R-018, R-019, R-020, R-021 |
| Before Phase 5 | - | R-022, R-023, R-024, R-025, R-026, R-044, R-047 |
| Before Phase 6 | - | R-064, R-065, R-066, R-067 |
| Before Phase 7 | - | R-068, R-069, R-070, R-071 |
| Before Phase 8 (later) | - | R-032, R-033, R-034, R-035, R-036, R-037 |

## Before Phase 1 (modular monolith)

| ID | Title | Status | Decision or options | ADR |
|----|-------|--------|---------------------|-----|
| R-001 | Two orders for the last unit at the same time | Decided | D: stock reservation as StockMovements, made atomic by A (R-001b). Option G added as a safety net: CHECK (0 <= Reserved <= OnHand), a violation answers 409. | [ADR-0001](decisions/ADR-0001-stock-reservation.md) |
| R-001b | How the stock reservation is modeled and protected | Decided | A: a Reserved counter in StockItem updated by one conditional statement, the movement added in the same transaction. Job every 5 minutes, 15-minute cut-off. | [ADR-0001](decisions/ADR-0001-stock-reservation.md) |
| R-002 | Duplicate order from a double click or a client retry | Decided | Rate limiting: one create-order per user per minute (tunable). The limits are accepted. | [ADR-0002](decisions/ADR-0002-duplicate-orders-rate-limiting.md) |
| R-003 | Money and rounding errors | Decided | decimal(18,2) inside a Money value object (amount and currency code), several currencies. | [ADR-0003](decisions/ADR-0003-money-and-currencies.md) |
| R-004 | Dates and time zones | Decided | The server's local time (DateTime.Now). The same TZ in every container. | [ADR-0004](decisions/ADR-0004-server-local-time.md) |
| R-005 | Deleting data and audit trail | Decided | Nothing is deleted for now. Soft delete only. | [ADR-0005](decisions/ADR-0005-no-deletion-soft-delete-only.md) |
| R-006 | Unbounded list queries (memory) | Decided | Offset pagination. Page size 10 by default, 1000 at most, from Options. | [ADR-0006](decisions/ADR-0006-paging.md) |
| R-027 | IQueryable leaks persistence into the Application layer | Decided | IQueryable from repositories, executed through IAsyncQueryableExecuter. | [ADR-0007](decisions/ADR-0007-iqueryable-and-executer.md) |
| R-029 | Where the Hangfire server runs | Decided | Inside HttpApi.Host. No separate Hangfire project. | [ADR-0008](decisions/ADR-0008-hangfire-in-host-and-job-rules.md) |
| R-030 | Jobs are retried, so they may run twice or fail forever | Decided | Idempotent jobs, limited attempts, delays from Options. No overlap with the same arguments. Never killed or replaced for taking time. | [ADR-0008](decisions/ADR-0008-hangfire-in-host-and-job-rules.md) |
| R-031 | Hangfire dashboard exposure and job storage growth | Decided | Only from inside the server or localhost, on its own internal port bound to 127.0.0.1. Retention from Options. Its data expiry is an accepted exception to the no-delete rule. | [ADR-0009](decisions/ADR-0009-hangfire-dashboard-localhost-only.md), [ADR-0005](decisions/ADR-0005-no-deletion-soft-delete-only.md) |
| R-038 | Guid generation strategy and index cost | Decided | Guid.CreateVersion7(), verified by a one-million-row test. | [ADR-0010](decisions/ADR-0010-guid-identifiers.md) |
| R-039 | Plain Guid or strongly typed ids | Decided | Raw Guid always. | [ADR-0010](decisions/ADR-0010-guid-identifiers.md) |
| R-040 | Where files physically live | Decided | Docker named volume, owned by a dedicated Files service. | [ADR-0011](decisions/ADR-0011-file-storage-naming-and-access.md) |
| R-041 | File naming, folder layout and link to the record | Decided | A Guid per upload plus a validated extension. Folders by module and date. The record holds ModuleName, EntityName, EntityId. | [ADR-0011](decisions/ADR-0011-file-storage-naming-and-access.md) |
| R-042 | Unsafe or oversized uploads | Decided | Both: size limit and content-signature allow-list, plus a ClamAV scan. | [ADR-0012](decisions/ADR-0012-upload-safety-and-consistency.md) |
| R-043 | File and database out of sync after a crash | Decided | Record Pending first, write the file, then Active. A job marks stale Pending as Failed. | [ADR-0012](decisions/ADR-0012-upload-safety-and-consistency.md) |
| R-045 | How the request source is determined, and whether it can be faked | Decided | The client_id claim in the JWT. Anonymous calls are Unknown. | [ADR-0013](decisions/ADR-0013-request-source-from-client-id.md) |
| R-046 | How product search works | Decided | LIKE 'abc%' on a normalized search column (NormalizedName) with an index. | [ADR-0014](decisions/ADR-0014-product-search-like-prefix.md) |
| R-048 | Swagger exposure and tooling | Decided | Swagger enabled in every environment. Tooling: the built-in OpenAPI document (Microsoft.AspNetCore.OpenApi) with Swagger UI. | [ADR-0015](decisions/ADR-0015-swagger-in-every-environment.md) |
| R-049 | Audit columns: nullability and who acts with no user | Decided | All six nullable. A null means a record added by hand. | [ADR-0016](decisions/ADR-0016-nullable-audit-columns.md) |
| R-050 | Soft-delete purge and bulk operations that skip the interceptor | Decided | Nothing deleted for now. No ExecuteDelete. ExecuteUpdate sets the audit columns through a helper. | [ADR-0005](decisions/ADR-0005-no-deletion-soft-delete-only.md) |
| R-051 | Where the Redis, RabbitMQ and file adapters live | Decided | C: Shared.Infrastructure. | [ADR-0017](decisions/ADR-0017-shared-projects.md) |
| R-052 | CQRS and MediatR: adopt or not | Decided | A: no CQRS, no MediatR. Application Services. | [ADR-0018](decisions/ADR-0018-no-cqrs-no-mediatr.md) |
| R-053 | Branch and merge strategy | Decided | Pull Requests, squashed into main. main starts with the architecture. | [ADR-0019](decisions/ADR-0019-git-pull-requests-squash.md) |
| R-055 | Where the shared middleware lives | Decided | Shared.AspNetCore. | [ADR-0017](decisions/ADR-0017-shared-projects.md) |
| R-056 | Who owns the Translations table | Decided | A table per service. | [ADR-0021](decisions/ADR-0021-errors-translations-and-errordto.md) |
| R-057 | Shape of ErrorDto and ErrorCode | Decided | A code and a message only. | [ADR-0021](decisions/ADR-0021-errors-translations-and-errordto.md) |
| R-058 | Fallback when a message cannot be loaded | Decided | The last value in memory, else an empty message. | [ADR-0021](decisions/ADR-0021-errors-translations-and-errordto.md) |
| R-060 | Where an adapter that belongs to one service lives | Decided | Its own project: Payments.Infrastructure. | [ADR-0020](decisions/ADR-0020-one-solution-and-payments-infrastructure.md) |
| R-062 | Where the file storage adapter lives | Decided | Stays in Shared.Infrastructure. | [ADR-0011](decisions/ADR-0011-file-storage-naming-and-access.md) |
| R-063 | How several currencies work | Decided | One currency per order, no conversion. One price per currency per product. | [ADR-0003](decisions/ADR-0003-money-and-currencies.md) |

## Before Phase 2 (Redis)

| ID | Title | Status | Decision or options | ADR |
|----|-------|--------|---------------------|-----|
| R-007 | Redis is down | Open | Fail open (read from SQL Server); fail closed; a circuit breaker around cache calls. | - |
| R-008 | Stale cache after an update | Open | TTL only; explicit invalidation on write; both. | - |
| R-009 | Cache stampede when a hot key expires | Open | A lock per key while rebuilding; jittered TTLs; accept the risk. | - |
| R-010 | Redis memory fills up | Open | maxmemory with an eviction policy (allkeys-lru, volatile-lru, noeviction); keep the cart in Redis or in SQL Server. | - |
| R-059 | Editing translations and refreshing the cache | Open | An admin endpoint that updates the table and deletes the Redis hash; TTL only; a Redis pub/sub notification on change; reseed from files on every deploy. | - |

## Before Phase 3 (RabbitMQ)

| ID | Title | Status | Decision or options | ADR |
|----|-------|--------|---------------------|-----|
| R-011 | A message is delivered twice (at-least-once) | Open | Idempotent consumers with an inbox table; de-duplicate by message id; accept duplicates. | - |
| R-012 | Order saved but the event was not published | Open | Outbox pattern; publish first then save (unsafe); accept possible loss. | - |
| R-013 | A poison message that fails forever | Open | Retry count then a dead-letter queue; discard; a manual review process. | - |
| R-014 | Events arrive out of order | Open | Ignore ordering; version numbers on events; a single active consumer. | - |
| R-015 | Consumer memory grows | Open | Prefetch count; maximum concurrent consumers; message size limit. | - |
| R-028 | A RabbitMQ consumer is a long-running listener, not a job | Open | A listener that only enqueues a Hangfire job (ids only) and then acknowledges the message (proposed in the plan); process inline in the listener; decide exactly when to acknowledge. | - |

## Before Phase 4 (services)

| ID | Title | Status | Decision or options | ADR |
|----|-------|--------|---------------------|-----|
| R-016 | No single transaction across stock (Catalog) and orders (Orders) | Open | Saga with choreography; saga with orchestration; reservation with expiry (events with the reservation movements); keep stock and orders in one service. | - |
| R-017 | A service call is slow or fails | Open | Timeouts, retries with jitter and a circuit breaker (resilience handlers); fail fast. | - |
| R-018 | Breaking change in a Domain.Shared.Layer contract | Open | Versioned events (V1, V2); additive-only changes; a versioned NuGet package. | - |
| R-019 | One SQL Server container or one per service | Open | One container with several databases (less RAM, less isolation); one per service (more RAM, more isolation). | - |
| R-020 | Service-to-service authentication | Open | Client-credentials JWT; mutual TLS; network isolation only. | - |
| R-021 | JWT signing key leak or rotation | Open | A symmetric key from the environment; asymmetric keys with a key endpoint; Keycloak. | - |
| R-054 | One solution per service or one for everything | Decided | One big solution, OrderFlow.sln. | [ADR-0020](decisions/ADR-0020-one-solution-and-payments-infrastructure.md) |
| R-061 | How the Files service authorizes access to a file of another service's record | Decided | An Admin, or whoever can see the record, checked through IFileOwnerAccessChecker. | [ADR-0011](decisions/ADR-0011-file-storage-naming-and-access.md) |

## Before Phase 5 (production readiness and Seq)

| ID | Title | Status | Decision or options | ADR |
|----|-------|--------|---------------------|-----|
| R-022 | Log storage grows without limit | Open | Retention in days in Seq; the production log level; the volume size. | - |
| R-023 | Sensitive data ends up in logs | Open | Masking or destructuring policies; never log whole DTOs; a review checklist. | - |
| R-024 | A bad release must be rolled back | Open | The previous image tag; backward-compatible migrations (expand then contract); a backup before migrating. | - |
| R-025 | Secrets in the CI pipeline | Open | GitHub encrypted secrets and environments; OIDC to the registry. | - |
| R-026 | Vulnerable base images | Open | Scan images in CI (for example Trivy or Docker Scout) with a fail threshold. | - |
| R-044 | The files volume (and SQL Server) is lost or corrupted | Open | A scheduled volume backup to another disk or location; replicate to a second place; a restore drill that is actually tested. | - |
| R-047 | Index maintenance (fragmentation and statistics) | Open | A Hangfire recurring job that reorganizes or rebuilds above thresholds from Options; SQL Agent scripts; rely on automatic statistics only for now. | - |

## Before Phase 6 (security tests)

| ID | Title | Status | Decision or options | ADR |
|----|-------|--------|---------------------|-----|
| R-064 | What the security tests cover, and with which tools | Open | Scope: the API through the gateway only (anonymous and every role); plus the containers and images; plus code and dependency analysis. Tools: OWASP ZAP, Trivy, dotnet list package --vulnerable, gitleaks, CodeQL. Manual cases for authorization, tokens, IDOR and uploads. A fail threshold per severity. | - |
| R-065 | Where the security tests run and with what data | Open | An isolated Compose stack with fake data and seeded users, reset between runs (proposed); the normal development stack (an active scan changes data); never real data and never a shared server. | - |
| R-066 | How findings are handled | Open | High and Critical fixed before the merge; Medium fixed or accepted with an ADR; Low recorded; accepted risks such as Swagger in Production (R-048) re-confirmed with evidence; everything in docs/security-findings.md. | - |
| R-067 | Reviewing security tests written by AI | Open | Every negative test is seen failing before the fix; Karam reviews the authorization matrix and the attack cases; established tools run next to them; a second independent check on the highest risks. | - |

## Before Phase 7 (performance tests)

| ID | Title | Status | Decision or options | ADR |
|----|-------|--------|---------------------|-----|
| R-068 | Performance targets and scenarios | Open | Decide the targets now (users, requests per second, p95 and p99 latency, error rate, memory per container); or measure first and set the targets afterwards. | - |
| R-069 | Performance tools and where they run | Open | k6 in a container (JavaScript); NBomber (C#). Either can run a short smoke test in CI. | - |
| R-070 | Making the numbers fair | Open | Fixed cpus and memory per container from .env; a warm-up; several runs and the median; nothing else heavy running (including the IDE and the Claude Code session). | - |
| R-071 | Data volume, and the rate limit during tests | Open | Seed realistic volumes with a DbMigrator seed profile. For the create-order limit: a test profile raises it through Options, or the test uses many users. | - |

## Before Phase 8 (later, Vue.js frontend, not in scope now)

| ID | Title | Status | Decision or options | ADR |
|----|-------|--------|---------------------|-----|
| R-032 | Where the browser keeps the login token | Open | In memory with a refresh token; an httpOnly cookie through a backend-for-frontend (BFF); local storage (exposed to script attacks). | - |
| R-033 | CORS and origins | Open | Frontend and API on the same origin behind the gateway; different origins with an allow-list from Options. | - |
| R-034 | Frontend and API drift apart | Open | A typed client generated from OpenAPI; a hand-written client with contract tests. | - |
| R-035 | UI permissions mistaken for security | Open | The backend enforces everything; the UI only hides what the user cannot use; tests on the API for each role. | - |
| R-036 | How the frontend is served | Open | An nginx container in Compose; served by the gateway; static hosting outside Compose. | - |
| R-037 | Rendering needs | Open | A plain Vue single-page app; add server-side rendering (for example Nuxt) only if search engines or first-load speed require it. | - |

## Open points inside decided risks

These are details the decided ADRs leave open. They must be decided by Karam before the phase that
needs them.

| Point | Where | Needed by |
|-------|-------|-----------|
| Money amount with more than 2 decimal places (reject or round); currencies with 3 minor units | ADR-0003 | Phase 1 |
