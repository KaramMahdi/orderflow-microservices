# ADR-0015: Swagger is enabled in every environment

Date: 2026-11-24

Status: Accepted

Decided by: Karam

Risks: R-048

## Context

Swagger UI lets anyone call every endpoint from the browser with a token. Enabled in Production, it
shows every endpoint and schema to anyone who can reach the host.

## Options considered

- A. Enabled only in Development and Staging, through an Options flag.
- B. Enabled in Production but protected for Admins.
- C. Off everywhere except locally.
- D. Enabled in every environment.
- Tooling: Swashbuckle, or the built-in OpenAPI document with Swagger UI.

## Decision

Option chosen: **D, Swagger in every environment, including Production**, through an Options flag
that stays on, with a Bearer **Authorize** button.

Tooling (decided by Karam on 2026-11-24): **the built-in OpenAPI document with Swagger UI.**

- `Microsoft.AspNetCore.OpenApi` generates the OpenAPI document (`MapOpenApi()`).
- `Swashbuckle.AspNetCore.SwaggerUI` (the UI package only) shows it (`UseSwaggerUI(...)`).
- The full Swashbuckle generator is not used.
- The Bearer security scheme (the Authorize button), the `ErrorDto` error responses and the
  `ErrorCode` enum are added through OpenAPI document transformers.
- The exact package versions and APIs are checked at the start of Phase 1.

## Consequences

- Every endpoint and schema is visible to anyone who can reach the host. Calls still need a valid
  token, and tokens are never logged.
- A reflection test fails when any controller action has neither `[Authorize]` nor
  `[AllowAnonymous]`, so nothing is public by accident.
- Error responses are documented once for every action with the `ErrorDto` schema. `ErrorCode` is
  published as an enum in the OpenAPI document.
- From Phase 4 each service publishes its own OpenAPI document, and the gateway shows one Swagger
  page listing them all.
- Phase 6 re-confirms this decision with evidence (R-066).
- The built-in generator follows Microsoft's direction for .NET 9 and 10 and supports OpenAPI 3.1.
  It has fewer examples than Swashbuckle, and its customization uses transformers, not filters.

## Revisit when

Phase 6 security findings show the exposure is not acceptable.
