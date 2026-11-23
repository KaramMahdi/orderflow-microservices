# ADR-0022: Naming and layout: Domain.Shared.Layer and the DealerOperations-style project split

Date: 2026-11-24

Status: Accepted

Decided by: Karam

Risks: none (a layout decision)

## Context

The solution needs one project split, used the same way in every context, and one name for the
common library. Karam's DealerOperations solution already uses an ABP-style layered split and a
common library named `Domain.Shared.Layer`. That solution also has a separate Windows service
project for Hangfire.

## Options considered

- A. Follow the DealerOperations split project for project, with the same common library name.
- B. A different layout (for example a single Infrastructure project per service). The plans record
  this layout as dropped (R-051).
- For Hangfire: a separate host project like the DealerOperations Windows service, or none (R-029).

## Decision

Option chosen: **A.**

- The common library is named `Domain.Shared.Layer`.
- Every context uses the same set: `.Domain.Shared`, `.Domain`, `.Application.Contracts`,
  `.Application`, `.EntityFrameworkCore`, `.HttpApi`, `.HttpApi.Client`, `.HttpApi.Host`, `.DbMigrator`.
- Phase 1 uses one set with the prefix `OrderFlow.*`, with the contexts as modules (folders).
  Phase 4 splits it into Identity, Catalog, Orders, Payments, Notifications and Files, plus the Gateway.
- No separate Hangfire project: the Hangfire server runs inside `HttpApi.Host` (ADR-0008).

## Consequences

- The per-context project is `OrderFlow.Domain.Shared` (later `Orders.Domain.Shared` and so on). It
  holds the enums, constants, error codes and limits of that context only. It must not be confused
  with the common `Domain.Shared.Layer`.
- Reference rules are checked by architecture tests (see AGENTS.md).

## Revisit when

The layout causes friction that a new ADR must address.
