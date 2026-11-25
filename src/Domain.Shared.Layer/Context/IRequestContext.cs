namespace Domain.Shared.Layer.Context;

/// <summary>
/// The per-request context filled by a middleware (ADR-0013). Application Services read it;
/// the Domain never sees it.
/// </summary>
public interface IRequestContext
{
    /// <summary>
    /// The id that follows the request through logs, messages and jobs (X-Correlation-ID).
    /// A string, so any header value a client sends is accepted (ADR-0013).
    /// </summary>
    string CorrelationId { get; }

    /// <summary>The authenticated user, or null for an anonymous call.</summary>
    Guid? UserId { get; }

    /// <summary>The client app that sent the request, from the client_id claim.</summary>
    RequestSource Source { get; }

    /// <summary>The version of the client app, when the client sends one.</summary>
    string? ClientVersion { get; }

    /// <summary>The language of the request (ar or en), from Accept-Language.</summary>
    string Language { get; }
}
