namespace Domain.Shared.Layer.Localization;

/// <summary>
/// Identifies one text in the Translations table: the module, the optional list and the name (ADR-0021).
/// </summary>
public sealed record MessageKey
{
    /// <summary>Creates a message key.</summary>
    /// <param name="module">The module, one of <see cref="Constants.Modules"/>.</param>
    /// <param name="name">The name of the message inside the module (or list).</param>
    /// <param name="list">The optional list that groups messages inside the module.</param>
    public MessageKey(string module, string name, string? list = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(module);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Module = module;
        Name = name;
        List = list;
    }

    /// <summary>The module that owns the message.</summary>
    public string Module { get; }

    /// <summary>The name of the message.</summary>
    public string Name { get; }

    /// <summary>The optional list that groups messages inside the module.</summary>
    public string? List { get; }
}
