# ADR-0012: Upload safety and file-database consistency

Date: 2026-11-24

Status: Accepted

Decided by: Karam

Risks: R-042, R-043

## Context

Uploads can be unsafe (malware, a wrong type, oversized). A crash between writing the file and
saving the record can leave the disk and the database out of sync.

## Options considered

R-042 (unsafe or oversized uploads):

- A. An allow-list checked against the real content signature, plus size limits.
- B. An antivirus scan (a ClamAV container) before the file becomes Active.
- C. Both.

R-043 (out of sync after a crash):

- A. Record Pending first, write the file, mark Active, with a cleanup job for stale Pending.
- B. Write the file first, then the metadata, with a reconciliation job.
- C. An outbox.

## Decision

Option chosen: **R-042 C (both) and R-043 A (Pending first).**

1. A `FileRecord` is created with a new Guid, status **Pending**.
2. The stream is written under the unique name.
3. Size and SHA-256 are saved, and ClamAV scans the stream.
4. Clean: status **Active**. Infected or not allowed: status **Rejected**, never served.
5. A thin Hangfire job marks stale Pending records as **Failed**. It deletes nothing.

Limits, allowed content types, the root path, the Pending timeout and the scan settings come from
Options.

## Consequences

- ClamAV needs a lot of RAM for its signatures: its container gets its own memory limit, checked with
  `docker stats`. It needs time to load signatures, so uploads wait or answer 503 until it is healthy.
- The scan streams the file to the ClamAV daemon; the file never loads whole into memory.
- A Failed or Rejected file stays on disk (ADR-0005).
- Tests: EICAR is rejected, a content type that disagrees with the signature is rejected, an oversize
  file is rejected, a Pending or Rejected file is never served, the stale-Pending job.

## Revisit when

Upload volume or scan latency becomes a problem (Phase 7 measurements).
