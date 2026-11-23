# ADR-0008: Hangfire runs inside the Host, jobs never overlap and are never killed

Date: 2026-11-24

Status: Accepted

Decided by: Karam

Risks: R-029, R-030

## Context

All background work runs as Hangfire jobs (golden rule 12). Two questions: where the Hangfire
server runs (R-029), and what happens when a job is retried, runs twice, runs long or fails forever
(R-030).

## Options considered

R-029 (where the server runs):

- A. Inside the API process (HttpApi.Host).
- B. A separate host project per service (a Windows service cannot run in Linux containers).
- C. One shared jobs service.

R-030 (retries, duplicates, long runs): idempotent jobs that check the state before acting, with
limited attempts and delays from Options, plus Karam's two rules on overlap and long runs.

## Decision

Option chosen: **R-029 A, and the R-030 rules below.**

- The Hangfire server (and its dashboard, ADR-0009) runs inside `HttpApi.Host`. There is no separate
  Hangfire project. `WorkerCount` is capped from Options so jobs do not starve the API of memory.
- A job never starts while a run of the same job with the same arguments is running. A job filter
  skips the new run.
- No job is ever killed or replaced because it took time. The storage keeps long jobs invisible while
  they run.
- Jobs are idempotent: they check the state before acting.
- Attempts are limited, and the delays come from Options.
- Jobs are thin: one call to an Application Service, arguments are ids only.

## Consequences

- The API and the jobs share one process and its memory. Container memory limits must cover both.
- An `ErrorException` with a code below 500 is not retried (it would fail again). Other exceptions
  follow the retry policy.
- A Hangfire filter logs every failure with the job id and the CorrelationId, and copies the
  CorrelationId and the request source into the job.
- Tests: the no-overlap filter (a second run with the same arguments is skipped), idempotency of
  each job, and the retry policy.

## Revisit when

Jobs need more resources than the API process can give (Phase 7 measurements), or Phase 4 when
each service hosts its own Hangfire server.
