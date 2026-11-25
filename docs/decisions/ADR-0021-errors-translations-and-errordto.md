# ADR-0021: Errors: a Translations table per service, ErrorDto with a code and a message, and the fallback

Date: 2026-11-24

Status: Accepted

Decided by: Karam

Risks: R-056, R-057, R-058

## Context

Every error that leaves an API must have the same shape and be produced in one place (golden rule
20). Messages are not hard-coded (golden rule 8): they come from a Translations table in Arabic and
English. It must be decided who owns that table (R-056), the shape of `ErrorDto` (R-057), and what
happens when a message cannot be loaded (R-058).

## Options considered

R-056 (who owns the Translations table): one table in each service's own database; one central
Localization service with its own database and API, read through Redis; translations as files shipped
with each service.

R-057 (shape of ErrorDto and ErrorCode): code and message only, with HTTP-like numbers; an
optional Key so clients can tell two errors with the same code apart; business codes such as 4001 next
to the HTTP-like ones; field-level validation details later.

R-058 (fallback when a message cannot be loaded): embedded minimal messages in the shared project
(an exception to rule 8); return the code with an empty message; reuse the last cached value.

## Decision

- **R-056:** a Translations table per service, in that service's own database.
- **R-057:** `ErrorDto` is a code and a message only. `ErrorCode` values are the HTTP status numbers
  (400, 401, 403, 404, 409, 422, 429, 500, 503).
- **R-058:** if a message cannot be loaded, the last value kept in memory is used; if there is none, the
  `ErrorDto` is returned with the code and an empty message.

## Consequences

- Business code throws `ErrorException(ErrorCode, MessageKey, args)`. The Domain never sets an
  HTTP status or writes a message text.
- One shared handler in Shared.AspNetCore turns every exception (and 401, 403, 404) into an
  `ErrorDto`, in the request language (Accept-Language, default English). No stack trace, exception
  text, SQL, type names or file paths reach the client. The client gets the `X-Correlation-ID` header.
- Lookup order: requested language, then English, then the Common message for the code. A missing
  translation is logged as a Warning. The handler never throws. The memory cache has a size limit.
- The Translations table: Id, ModuleName, ListName (optional), Name, Language (ar or en), Value
  (nvarchar with a maximum length, never max), and the six audit columns. Seeded by the DbMigrator
  from one JSON file per module and language.
- **Language codes are string constants (decided by Karam, 2026-11-26):** the `Languages` class in
  Domain.Shared.Layer holds `ar`, `en`, the default (`en`) and the supported list. The Language column
  stores the two-letter code and `IRequestContext.Language` is a string; no enum.
- The frontend and mobile clients define the same `ErrorCode` values and show their own text for a
  code with an empty message.
- Tests: each exception type gives the expected code and status; an unknown exception returns the
  generic message with no details; the language header selects Arabic or English; 401, 403 and 404
  return an `ErrorDto`; a missing translation falls back; a completeness test fails if any `MessageKey`
  lacks an Arabic or English row in the seed files.

## Revisit when

Phase 2 (Redis caching of translations, R-059 open), or clients need to tell two errors with the same
code apart.
