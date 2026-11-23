# ADR-0019: Git: main starts with the architecture, each phase reaches main through a squashed Pull Request

Date: 2026-11-24

Status: Accepted

Decided by: Karam

Risks: R-053

## Context

The project is delivered phase by phase. The branch and merge strategy decides what `main` shows
and how a phase can be reviewed and found again later.

## Options considered

- A. Phase branches chained, `main` receives the last phase at the end.
- B. Merge each finished phase into `main`.
- Merge style: squash, merge commit, or rebase.

## Decision

Option chosen: **B with squash, through Pull Requests.**

- `main` starts with the architecture and AGENTS.md only (Phase 0, no code).
- Each phase has its own branch (`phase-N-<name>`), created from `main` after the previous phase
  is merged, and its own root README.
- Each phase reaches `main` through one Pull Request, squashed into one commit.
- A phase is finished in this order: the README is written and tested from a clean clone, the Pull
  Request is merged, then the tag is created (`v1-monolith`, `v2-redis`, `v3-rabbitmq`,
  `v4-services`, `v5-production`, `v6-security`, `v7-performance`).

## Consequences

- `main`'s README always describes the latest merged phase. Earlier READMEs stay in the tags.
- The history of `main` has one commit per phase. The detailed history lives in the Pull Requests.
- Pull Request checklist: the golden rules hold, the tests are green, the README was tested from a
  clean clone, the ADRs of the phase are updated (see AGENTS.md).

## Revisit when

The team grows and needs a different flow (for example several Pull Requests per phase).
