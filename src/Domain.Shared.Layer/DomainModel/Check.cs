using Domain.Shared.Layer.Errors;
using Domain.Shared.Layer.Localization;

namespace Domain.Shared.Layer.DomainModel;

/// <summary>
/// Input checks shared by the factory methods of the domain. A failed check throws an
/// <see cref="ErrorException"/> with <see cref="ErrorCode.ValidationFailed"/> and the given key;
/// the field name and the limit are the message arguments.
/// </summary>
public static class Check
{
    /// <summary>Returns the trimmed text, or throws when it is empty or longer than the limit.</summary>
    /// <param name="value">The text.</param>
    /// <param name="field">The field name, a message argument.</param>
    /// <param name="maxLength">The maximum length after trimming.</param>
    /// <param name="requiredKey">The key used when the text is empty.</param>
    /// <param name="tooLongKey">The key used when the text is too long.</param>
    public static string Text(string? value, string field, int maxLength, MessageKey requiredKey, MessageKey tooLongKey)
    {
        var trimmed = value?.Trim();

        if (string.IsNullOrEmpty(trimmed))
        {
            throw new ErrorException(ErrorCode.ValidationFailed, requiredKey, field);
        }

        return MaxLength(trimmed, field, maxLength, tooLongKey);
    }

    /// <summary>Returns the trimmed text or null when it is empty; throws when it is longer than the limit.</summary>
    /// <param name="value">The text.</param>
    /// <param name="field">The field name, a message argument.</param>
    /// <param name="maxLength">The maximum length after trimming.</param>
    /// <param name="tooLongKey">The key used when the text is too long.</param>
    public static string? OptionalText(string? value, string field, int maxLength, MessageKey tooLongKey)
    {
        var trimmed = value?.Trim();

        return string.IsNullOrEmpty(trimmed) ? null : MaxLength(trimmed, field, maxLength, tooLongKey);
    }

    /// <summary>Returns the id, or throws when it is <see cref="Guid.Empty"/>.</summary>
    /// <param name="value">The id.</param>
    /// <param name="field">The field name, a message argument.</param>
    /// <param name="requiredKey">The key used when the id is empty.</param>
    public static Guid NotEmpty(Guid value, string field, MessageKey requiredKey)
    {
        if (value == Guid.Empty)
        {
            throw new ErrorException(ErrorCode.ValidationFailed, requiredKey, field);
        }

        return value;
    }

    private static string MaxLength(string value, string field, int maxLength, MessageKey tooLongKey)
    {
        if (value.Length > maxLength)
        {
            throw new ErrorException(ErrorCode.ValidationFailed, tooLongKey, field, maxLength);
        }

        return value;
    }
}
