namespace Shared.AspNetCore.Constants;

/// <summary>
/// Paths of the technical endpoints every host exposes.
/// </summary>
public static class EndpointPaths
{
    /// <summary>Health endpoint, used by the container healthcheck.</summary>
    public const string Health = "/health";
}
