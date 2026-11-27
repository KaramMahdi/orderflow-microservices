namespace OrderFlow.Domain.Shared.Files;

/// <summary>
/// The state of a file record (ADR-0012). Only Active files are served.
/// Stored as a tinyint: the numbers are explicit and never reordered.
/// </summary>
public enum FileStatus : byte
{
    /// <summary>The record exists; the content is being written and scanned.</summary>
    Pending = 1,

    /// <summary>The content is saved and clean. The only status that is served.</summary>
    Active = 2,

    /// <summary>Stayed Pending too long; marked by the cleanup job.</summary>
    Failed = 3,

    /// <summary>Infected or not allowed. Never served.</summary>
    Rejected = 4,
}
