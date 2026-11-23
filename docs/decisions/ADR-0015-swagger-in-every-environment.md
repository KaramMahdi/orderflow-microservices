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
- **Not decided here:** the tooling (Swashbuckle or the built-in OpenAPI document with Swagger UI).
  It is chosen in Phase 1 after checking the current package versions.

## Revisit when

Phase 6 security findings show the exposure is not acceptable.
