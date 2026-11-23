# ADR-0020: One big solution, and Payments.Infrastructure for the payment provider adapter

Date: 2026-11-24

Status: Accepted

Decided by: Karam

Risks: R-054, R-060

## Context

From Phase 4 each service has its own set of projects. It must be decided whether they live in one
solution or several (R-054), and where an adapter that belongs to one service lives, for example the
payment provider (R-060).

## Options considered

R-054:

- A. One solution per service, each with its own set of projects like DealerOperations.
- B. One big solution with all services.
- The shared library as a NuGet package or a project reference.

R-060:

- A. A folder `Adapters` inside that service's HttpApi.Host.
- B. Its own project, such as `Payments.Infrastructure`.
- C. Inside that service's Application, behind an interface (blurs the layers).

## Decision

Option chosen: **R-054 B and R-060 B.**

- One solution, `OrderFlow.sln`, holds everything: about 9 projects per service (around 55 with six
  services), the 3 common projects and the gateway. Shared projects are project references.
- `Payments.Infrastructure` holds the payment provider adapter: the anti-corruption layer that
  translates the provider's model into the Payments model. It references `.Application` and `.Domain`.

## Consequences

- A large solution. It works only if every service stays clear, understandable and clean, with the
  same structure and the same names.
- Builds and architecture tests cover all services at once.
- `.Infrastructure` projects exist only when a service needs one.

## Revisit when

The solution becomes too slow to work with, or the services move to separate repositories.
