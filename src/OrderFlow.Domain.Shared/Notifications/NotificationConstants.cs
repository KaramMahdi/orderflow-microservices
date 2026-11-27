namespace OrderFlow.Domain.Shared.Notifications;

/// <summary>Limits of the Notifications model.</summary>
public static class NotificationConstants
{
    /// <summary>The maximum length of a recipient address (email or phone number).</summary>
    public const int RecipientAddressMaxLength = 256;

    /// <summary>The maximum length of a subject.</summary>
    public const int SubjectMaxLength = 200;

    /// <summary>The maximum length of a body.</summary>
    public const int BodyMaxLength = 4000;

    /// <summary>The maximum length of the last error; longer errors are cut.</summary>
    public const int LastErrorMaxLength = 1000;
}
