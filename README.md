# OrderFlow

OrderFlow is an order-management reference project built with full Domain-Driven Design on .NET 10,
SQL Server and Docker. The domain is kept small on purpose (products, stock, customers, orders,
payments, notifications, files), so the effort goes into architecture, infrastructure and Docker, not
into business rules.

The project is delivered phase by phase. Each phase adds one new thing on top of the previous one,
and no phase starts before its risks have a written decision (an ADR).

> **Current state: Phase 0 (foundation).** This repository holds the architecture documents and the
> instructions for AI tools only. There is no source code yet. Code starts in Phase 1.

## Stack

.NET 10 (ASP.NET Core, EF Core), full DDD, SQL Server, Docker Compose, Hangfire, Mapster,
Serilog, and later Redis (Phase 2), RabbitMQ (Phase 3), YARP (Phase 4) and Seq (Phase 5).

## Phases

| # | Phase | Branch | Tag | What it adds |
|---|-------|--------|-----|--------------|
| 0 | Foundation | `main` (first commit) | - | Architecture documents, ADRs, glossary, context map, AGENTS.md. No code |
| 1 | Modular monolith | `phase-1-modular-monolith` | `v1-monolith` | Full DDD solution on SQL Server: Auth, Swagger, error handling, Hangfire, stock reservation, files, rate limiting, indexes, audit columns |
| 2 | Redis | `phase-2-redis` | `v2-redis` | Caching of products, cart and translations, with memory limits and fallbacks |
| 3 | RabbitMQ | `phase-3-rabbitmq` | `v3-rabbitmq` | Domain events become integration events, the listener enqueues Hangfire jobs, notifications, invoice PDFs |
| 4 | Services | `phase-4-services` | `v4-services` | Six services and a YARP gateway in one solution, typed clients, one database per service |
| 5 | Production and Seq | `phase-5-production-and-seq` | `v5-production` | CI/CD, registry, Seq logs, OpenTelemetry, backups, index maintenance |
| 6 | Security tests | `phase-6-security-tests` | `v6-security` | The Compose stack tested like an attacker: ZAP, Trivy, authorization and token tests, upload abuse |
| 7 | Performance tests | `phase-7-performance-tests` | `v7-performance` | Load, contention, soak and spike tests on fixed resources, tuning with evidence |
| 8 | Vue.js frontend (later) | `phase-8-vue-frontend` | - | The website. Not in scope now |

## Branch model

```
main (first commit: architecture + AGENTS.md, no code)
 |
 +-- phase-1-modular-monolith    -- PR (squash) --> main, tag v1-monolith
 +-- phase-2-redis               -- PR (squash) --> main, tag v2-redis
 +-- phase-3-rabbitmq            -- PR (squash) --> main, tag v3-rabbitmq
 +-- phase-4-services            -- PR (squash) --> main, tag v4-services
 +-- phase-5-production-and-seq  -- PR (squash) --> main, tag v5-production
 +-- phase-6-security-tests      -- PR (squash) --> main, tag v6-security
 +-- phase-7-performance-tests   -- PR (squash) --> main, tag v7-performance
 +-- phase-8-vue-frontend        (later, not in scope now)
```

- `main` starts with the architecture and AGENTS.md only.
- Each phase has its own branch, created from `main` after the previous phase is merged.
- Each phase has its own root README that a stranger can follow from a clean clone. After the merge,
  the README on `main` describes the latest merged phase. Earlier READMEs stay in the phase tags.
- Each phase reaches `main` through one Pull Request, squashed into one commit.
- A phase is finished in this order: README written and tested from a clean clone, Pull Request
  merged, then the tag is created.

The loop every phase follows: **Decide** (an ADR for every risk of the phase), **Branch**, **Build**
(milestones in order, each with its tests), **Document**, **Prove it** (follow the README from a clean
clone), **Merge** (Pull Request, squash), **Tag**. See [AGENTS.md](AGENTS.md) for the checklist.

## Documentation (`docs/`)

| Path | What it holds |
|------|---------------|
| [docs/risks.md](docs/risks.md) | The risk register: every risk, its status (Decided or Open), and the decision or the options |
| [docs/decisions/](docs/decisions/) | Architecture Decision Records, one per decision group (ADR-0001 onward) |
| [docs/glossary.md](docs/glossary.md) | The ubiquitous language of each bounded context |
| [docs/context-map.md](docs/context-map.md) | The bounded contexts, their subdomain type and their relationships |
| [docs/event-storming.md](docs/event-storming.md) | Domain events, commands, aggregates and contexts (draft) |
| [docs/indexes.md](docs/indexes.md) | The index catalog: every index, its properties and the query it serves |
| [docs/orderflow-phases-plan.pdf](docs/orderflow-phases-plan.pdf) | The working plan, phase by phase |
| [docs/docker-learning-project-plan.pdf](docs/docker-learning-project-plan.pdf) | The long reference plan (architecture, risks, patterns) |

The documents grow phase by phase. A decision is never edited away: a new ADR supersedes the old one.

## For AI tools

[AGENTS.md](AGENTS.md) holds the golden rules, the solution structure, the conventions, the
decisions summary and the workflow. Every AI tool working in this repository must follow it.

## License

No license. All rights reserved. The code is published for reading and reference only.
