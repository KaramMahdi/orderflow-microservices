namespace Domain.Shared.Layer.Localization;

/// <summary>
/// The languages of the Translations table and of a request (ADR-0021), as the two-letter codes used in
/// the Language column and in Accept-Language. English is the default when the header is missing or
/// names a language that is not supported.
/// </summary>
public static class Languages
{
    /// <summary>Arabic.</summary>
    public const string Arabic = "ar";

    /// <summary>English.</summary>
    public const string English = "en";

    /// <summary>The language used when a request asks for none or for an unsupported one (ADR-0021).</summary>
    public const string Default = English;

    /// <summary>Every supported language. Each message key has a translation in each of them.</summary>
    public static IReadOnlyList<string> Supported { get; } = [Arabic, English];

    /// <summary>Returns true when the code is a supported language (case-insensitive).</summary>
    /// <param name="language">A language code, for example from Accept-Language.</param>
    public static bool IsSupported(string? language) =>
        language is not null && Supported.Contains(language, StringComparer.OrdinalIgnoreCase);
}
