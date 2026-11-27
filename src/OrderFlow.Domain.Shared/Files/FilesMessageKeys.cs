using Domain.Shared.Layer.Constants;
using Domain.Shared.Layer.Localization;

namespace OrderFlow.Domain.Shared.Files;

/// <summary>Message keys of the Files module (ADR-0021).</summary>
public static class FilesMessageKeys
{
    /// <summary>A required field is empty. Arguments: the field name.</summary>
    public static readonly MessageKey FieldRequired = new(Modules.Files, nameof(FieldRequired));

    /// <summary>A field is too long. Arguments: the field name, the maximum length.</summary>
    public static readonly MessageKey FieldTooLong = new(Modules.Files, nameof(FieldTooLong));

    /// <summary>The extension is not a dot followed by letters or digits. Arguments: the extension.</summary>
    public static readonly MessageKey ExtensionInvalid = new(Modules.Files, nameof(ExtensionInvalid));

    /// <summary>The saved content is empty. Arguments: the size.</summary>
    public static readonly MessageKey SizeNotPositive = new(Modules.Files, nameof(SizeNotPositive));

    /// <summary>The SHA-256 is not 64 lowercase hexadecimal characters.</summary>
    public static readonly MessageKey Sha256Invalid = new(Modules.Files, nameof(Sha256Invalid));

    /// <summary>The file record is no longer pending. Arguments: the file id, its status.</summary>
    public static readonly MessageKey FileNotPending = new(Modules.Files, nameof(FileNotPending));
}
