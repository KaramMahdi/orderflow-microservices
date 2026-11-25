namespace Domain.Shared.Layer.Errors;

/// <summary>
/// Classifies an error. The values are the HTTP status numbers (ADR-0021); clients define the same values.
/// </summary>
public enum ErrorCode
{
    /// <summary>The request is malformed.</summary>
    BadRequest = 400,

    /// <summary>The caller is not authenticated.</summary>
    Unauthorized = 401,

    /// <summary>The caller is authenticated but not allowed.</summary>
    Forbidden = 403,

    /// <summary>The requested resource does not exist.</summary>
    NotFound = 404,

    /// <summary>The request conflicts with the current state.</summary>
    Conflict = 409,

    /// <summary>The input failed validation.</summary>
    ValidationFailed = 422,

    /// <summary>The caller sent too many requests (rate limiting).</summary>
    TooManyRequests = 429,

    /// <summary>An unexpected error.</summary>
    InternalServerError = 500,

    /// <summary>A dependency is not available.</summary>
    ServiceUnavailable = 503,
}
