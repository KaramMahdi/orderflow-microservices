# ADR-0011: File storage, naming, linking and access

Date: 2026-11-24

Status: Accepted

Decided by: Karam

Risks: R-040, R-041, R-061, R-062

## Context

No file content is stored in the database (golden rule 14). It must be decided where files
physically live, who owns them, how they are named and linked to their business record, who may
open them, and where the storage adapter lives.

## Options considered

R-040 (where files live): a Docker named volume on local disk; a bind mount or network share;
S3-compatible storage (MinIO container); cloud blob storage. Also: a dedicated Files service, or the
shared file abstraction used by each service.

R-041 (naming and linking): name = a Guid per upload plus a validated extension; folders by module
and date, or flat; the file record links to the business record (ModuleName, EntityName, EntityId), or
the business record keeps a FileId, or both.

R-061 (access to a file owned by a record in another service): Files checks the token roles and the
file's creator only; Files asks the owning service whether the user may read the record (typed
client); the owning service issues a short-lived signed link that Files verifies.

R-062 (where the file storage adapter lives): stays in Shared.Infrastructure; moves into the Files
service; the other adapters stay shared either way.

## Decision

- **R-040:** a Docker named volume, owned by a dedicated Files context (a module in Phase 1, its own
  service from Phase 4). Other contexts call it through its HttpApi.Client and keep at most a file Guid.
- **R-041:** the `FileRecord` holds `ModuleName`, `EntityName` and `EntityId`. The unique name is a
  Guid per upload (the FileRecord's own Id) plus an extension taken from the validated content type,
  never from the user's file name. Folders by module and date:
  `{RootPath}/{Module}/{yyyy}/{MM}/{fileId}{ext}`. The record stores the relative path; the root comes
  from Options. The original file name is kept in `OriginalName` for display and downloads only.
- **R-061:** an Admin can open any file. Anyone else can open a file only if they can see the record it
  belongs to. Files asks the owning service through `IFileOwnerAccessChecker` (declared in
  Domain.Shared.Layer, implemented by each owning service). If the owning service cannot answer,
  access is denied. In Phase 1 the check is a direct call to the owning module.
- **R-062:** the `IFileStorage` implementation stays in Shared.Infrastructure. The interface is
  declared in Domain.Shared.Layer with the other abstractions (ADR-0017). Wording clarified by Karam,
  2026-11-26.

## Consequences

- No foreign keys from `FileRecord` to the business record (it lives in another service).
- One record can have many files.
- `IFileStorage` is the only way to touch file content. Upload and download are streamed; never a
  `byte[]` or `MemoryStream` for a whole file. SHA-256 is computed while streaming.
- Files are served through the API with authorization checks, `nosniff` and attachment downloads,
  never from a public static folder.
- Logs carry file ids, sizes and durations, never content or file names.
- A model test forbids `VARBINARY(MAX)` and unbounded `byte[]` on entities.
- Tests: the access rule (Admin, owner, another customer), across services from Phase 4.

## Revisit when

Storage moves to MinIO, S3 or blob storage (only the `IFileStorage` implementation changes).
