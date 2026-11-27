# ADR-0013: The request source comes from the client_id claim

Date: 2026-11-24

Status: Accepted

Decided by: Karam

Risks: R-045

## Context

Every request has a source: Web, Mobile or Api (golden rule 16). It is used for logs, analytics,
support and orders. It must be decided how the source is determined, and whether it can be faked.

## Options considered

- A. The `client_id` claim in the JWT, one per client app (trusted, signed).
- B. An `X-Client-Type` header (informational, can be faked).
- C. Both, with the claim winning.
- D. User-Agent only (unreliable).
- For anonymous calls: record them as Unknown, or reject them.

## Decision

Option chosen: **A. Anonymous calls are Unknown.**

- One client id per app (web, mobile, api). The login request identifies the calling app, Identity puts
  its client id in the token, and unknown client ids are rejected.
- The `X-Client-Type` header is not used for the source.
- `RequestSource` is an enum in Domain.Shared.Layer, stored as a tinyint with explicit numbers that
  are never reordered: `Unknown = 0`, `Web = 1`, `Mobile = 2`, `Api = 3`.

## Consequences

- A middleware fills a per-request `IRequestContext` (CorrelationId, UserId, Source, ClientVersion,
  Language). Application Services read it through the interface; the Domain never sees it and gets
  the source as a plain value.
- `CorrelationId` is a string, not a Guid (decided by Karam, 2026-11-25), so any `X-Correlation-ID`
  header value a client sends is accepted. This is an accepted exception to golden rule 13: it is a
  tracing value, not the identifier of a record.
- The source flows into every log line, orders, audit rows, integration events and Hangfire jobs.
- Calls made from Swagger carry a token issued for the Api client, so they are recorded as Api.
- The source is never used for authorization or pricing unless it comes from the token.
- Tests: a token per client id gives the right source; an anonymous call is Unknown; an unknown
  client id is rejected at login; a missing `client_id` claim is tested in Phase 6.

## Revisit when

A new client app is added, or a minimum app version for Mobile is needed.
