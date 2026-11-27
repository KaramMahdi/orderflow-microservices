namespace OrderFlow.Domain.Shared.Notifications;

/// <summary>
/// The state of a notification. Stored as a tinyint: the numbers are explicit and never reordered.
/// </summary>
public enum NotificationStatus : byte
{
    /// <summary>Waiting to be sent, or to be tried again at NextAttemptAt.</summary>
    Pending = 1,

    /// <summary>Sent. Final.</summary>
    Sent = 2,

    /// <summary>Every allowed attempt failed. Final.</summary>
    Failed = 3,
}
