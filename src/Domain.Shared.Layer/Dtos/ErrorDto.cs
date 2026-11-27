using Domain.Shared.Layer.Errors;

namespace Domain.Shared.Layer.Dtos;

/// <summary>
/// What the client receives on error: a code and a message in the request language (ADR-0021).
/// The message is empty when no translation could be loaded.
/// </summary>
/// <param name="Code">The error code.</param>
/// <param name="Message">The translated message, or empty.</param>
public sealed record ErrorDto(ErrorCode Code, string Message);
