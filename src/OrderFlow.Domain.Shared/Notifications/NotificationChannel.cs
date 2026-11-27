namespace OrderFlow.Domain.Shared.Notifications;

/// <summary>
/// How a notification is sent. Stored as a tinyint: the numbers are explicit and never reordered.
/// </summary>
public enum NotificationChannel : byte
{
    /// <summary>An email.</summary>
    Email = 1,

    /// <summary>A text message.</summary>
    Sms = 2,
}
