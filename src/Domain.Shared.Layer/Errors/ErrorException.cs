using Domain.Shared.Layer.Localization;

namespace Domain.Shared.Layer.Errors;

/// <summary>
/// The exception business code throws (ADR-0021). It carries a code and a message key, never a text
/// or an HTTP status; the shared handler turns it into an <see cref="Dtos.ErrorDto"/>.
/// </summary>
public class ErrorException : Exception
{
    /// <summary>Creates the exception.</summary>
    /// <param name="code">The error code.</param>
    /// <param name="key">The key of the message in the Translations table.</param>
    /// <param name="args">The values formatted into the message.</param>
    public ErrorException(ErrorCode code, MessageKey key, params object?[] args)
        : base(key?.ToString())
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(args);

        Code = code;
        Key = key;
        Args = args;
    }

    /// <summary>The error code.</summary>
    public ErrorCode Code { get; }

    /// <summary>The key of the message in the Translations table.</summary>
    public MessageKey Key { get; }

    /// <summary>The values formatted into the message.</summary>
    public IReadOnlyList<object?> Args { get; }
}
