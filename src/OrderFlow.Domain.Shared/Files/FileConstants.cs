namespace OrderFlow.Domain.Shared.Files;

/// <summary>Limits and formats of the Files model (ADR-0011).</summary>
public static class FileConstants
{
    /// <summary>The maximum length of the name the user uploaded.</summary>
    public const int OriginalNameMaxLength = 255;

    /// <summary>The maximum length of a content type (RFC 6838).</summary>
    public const int ContentTypeMaxLength = 127;

    /// <summary>The maximum length of an extension, the dot included.</summary>
    public const int ExtensionMaxLength = 10;

    /// <summary>The length of a Guid in its default format (with hyphens).</summary>
    public const int GuidTextLength = 36;

    /// <summary>The maximum length of a storage name: the record Guid plus the extension.</summary>
    public const int StorageNameMaxLength = GuidTextLength + ExtensionMaxLength;

    /// <summary>The maximum length of a module name in the owner link.</summary>
    public const int ModuleNameMaxLength = 50;

    /// <summary>The maximum length of an entity name in the owner link.</summary>
    public const int EntityNameMaxLength = 100;

    /// <summary>The maximum length of the relative storage path ({Module}/{yyyy}/{MM}).</summary>
    public const int StoragePathMaxLength = ModuleNameMaxLength + 10;

    /// <summary>The length of a SHA-256 in lowercase hexadecimal.</summary>
    public const int Sha256Length = 64;

    /// <summary>The character that starts an extension.</summary>
    public const char ExtensionPrefix = '.';

    /// <summary>The separator of the relative storage path, the same on every platform.</summary>
    public const char PathSeparator = '/';

    /// <summary>The format of the year folder.</summary>
    public const string YearFolderFormat = "yyyy";

    /// <summary>The format of the month folder.</summary>
    public const string MonthFolderFormat = "MM";
}
