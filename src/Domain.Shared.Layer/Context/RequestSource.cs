namespace Domain.Shared.Layer.Context;

/// <summary>
/// Where a request came from, taken from the client_id claim (golden rule 16, ADR-0013).
/// Stored as a tinyint: the numbers are explicit and never reordered.
/// </summary>
public enum RequestSource : byte
{
    /// <summary>Anonymous call, or no client_id.</summary>
    Unknown = 0,

    /// <summary>The web client.</summary>
    Web = 1,

    /// <summary>The mobile client.</summary>
    Mobile = 2,

    /// <summary>A direct API client (Swagger included).</summary>
    Api = 3,
}
