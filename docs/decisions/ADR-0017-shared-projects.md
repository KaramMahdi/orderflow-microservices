# ADR-0017: Shared projects: Shared.Infrastructure and Shared.AspNetCore

Date: 2026-11-24

Status: Accepted

Decided by: Karam

Risks: R-051, R-055

## Context

The Redis, RabbitMQ and file storage adapters are needed by every service (R-051). The shared
middleware (error handling, CorrelationId, request context, language) is needed by every host
(R-055). Both must be written once, without pushing heavy packages into the Domain.

## Options considered

R-051 (where the Redis, RabbitMQ and file adapters live):

- A. One project per service, `OrderFlow.Infrastructure` (the earlier layout, dropped).
- B. Inside Domain.Shared.Layer (one place, but Domain, Application.Contracts and HttpApi.Client
  would then carry the Redis and RabbitMQ packages).
- C. A new common project, `Shared.Infrastructure` (abstractions stay in Domain.Shared.Layer, the
  packages only reach the hosts).

R-055 (where the shared middleware lives):

- A. A new common project, `Shared.AspNetCore`, referenced by every host.
- B. Inside Domain.Shared.Layer with a FrameworkReference (breaks the no-ASP.NET rule).
- C. A NuGet package shared by all repositories.

## Decision

Option chosen: **R-051 C and R-055 A.**

- `Shared.Infrastructure`: Redis cache (`ICacheService`), RabbitMQ publisher and listener
  (`IMessagePublisher`), file storage (`IFileStorage`), the Redis message provider. It never references
  a service; its listener dispatches to `IIntegrationEventHandler<T>` handlers that each service registers.
- `Shared.AspNetCore`: the exception handler and status-code pages, CorrelationId, request context,
  language, `AddSharedErrorHandling` and `UseSharedErrorHandling`. Referenced by every project that
  serves HTTP: the Phase 1 host, each service host, the gateway.
- The abstractions stay in Domain.Shared.Layer.

## Consequences

- Domain, Application, Application.Contracts and HttpApi.Client never reference the Redis or
  RabbitMQ packages. An architecture test checks it.
- If the repositories are ever split, both projects become NuGet packages.

## Revisit when

The services move to separate repositories.
