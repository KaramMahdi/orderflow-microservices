using Domain.Shared.Layer.DomainModel;
using Domain.Shared.Layer.Errors;
using OrderFlow.Domain.Notifications.Events;
using OrderFlow.Domain.Shared.Notifications;

namespace OrderFlow.Domain.Notifications;

/// <summary>
/// A message sent to a recipient after an event (glossary). Invariants: attempts never above the
/// maximum (from Options, passed in); only a Pending notification is sent or tried again; Sent and
/// Failed are final. Times are server local time read through <see cref="TimeProvider"/> (ADR-0004).
/// </summary>
public sealed class Notification : AggregateRoot
{
    private Notification(Guid id, Guid recipientId, NotificationChannel channel, string recipientAddress, string? subject, string body, DateTime nextAttemptAt)
        : base(id)
    {
        RecipientId = recipientId;
        Channel = channel;
        RecipientAddress = recipientAddress;
        Subject = subject;
        Body = body;
        Status = NotificationStatus.Pending;
        NextAttemptAt = nextAttemptAt;
    }

    /// <summary>The user who receives it.</summary>
    public Guid RecipientId { get; private set; }

    /// <summary>How it is sent.</summary>
    public NotificationChannel Channel { get; private set; }

    /// <summary>Where it is sent: an email address or a phone number.</summary>
    public string RecipientAddress { get; private set; }

    /// <summary>The subject, for channels that have one.</summary>
    public string? Subject { get; private set; }

    /// <summary>The text.</summary>
    public string Body { get; private set; }

    /// <summary>The state.</summary>
    public NotificationStatus Status { get; private set; }

    /// <summary>The number of tries so far.</summary>
    public int AttemptCount { get; private set; }

    /// <summary>When a pending notification is tried (again); null once it is final.</summary>
    public DateTime? NextAttemptAt { get; private set; }

    /// <summary>The error of the last failed try, cut to the maximum length.</summary>
    public string? LastError { get; private set; }

    /// <summary>Creates a pending notification, due now.</summary>
    /// <param name="recipientId">The user who receives it.</param>
    /// <param name="channel">The channel.</param>
    /// <param name="recipientAddress">The email address or phone number.</param>
    /// <param name="subject">The subject, optional.</param>
    /// <param name="body">The text.</param>
    /// <param name="timeProvider">The clock.</param>
    public static Notification Create(
        Guid recipientId,
        NotificationChannel channel,
        string? recipientAddress,
        string? subject,
        string? body,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);
        Check.NotEmpty(recipientId, nameof(RecipientId), NotificationsMessageKeys.FieldRequired);

        if (!Enum.IsDefined(channel))
        {
            throw new ErrorException(ErrorCode.ValidationFailed, NotificationsMessageKeys.ChannelInvalid, channel);
        }

        var address = Check.Text(recipientAddress, nameof(RecipientAddress), NotificationConstants.RecipientAddressMaxLength, NotificationsMessageKeys.FieldRequired, NotificationsMessageKeys.FieldTooLong);
        var validSubject = Check.OptionalText(subject, nameof(Subject), NotificationConstants.SubjectMaxLength, NotificationsMessageKeys.FieldTooLong);
        var validBody = Check.Text(body, nameof(Body), NotificationConstants.BodyMaxLength, NotificationsMessageKeys.FieldRequired, NotificationsMessageKeys.FieldTooLong);

        return new Notification(Guid.CreateVersion7(), recipientId, channel, address, validSubject, validBody, Now(timeProvider));
    }

    /// <summary>The notification was sent. Raises <see cref="NotificationSent"/>.</summary>
    public void MarkSent()
    {
        EnsurePending();

        AttemptCount++;
        Status = NotificationStatus.Sent;
        NextAttemptAt = null;
        AddDomainEvent(new NotificationSent(Id, RecipientId, Channel));
    }

    /// <summary>
    /// A try failed. Below the maximum, the notification stays Pending until now plus the delay;
    /// at the maximum it becomes Failed and <see cref="NotificationFailed"/> is raised.
    /// </summary>
    /// <param name="error">What went wrong; cut to the maximum length.</param>
    /// <param name="maxAttempts">The maximum number of tries (Options), above zero.</param>
    /// <param name="retryDelay">The wait before the next try (Options), not negative.</param>
    /// <param name="timeProvider">The clock.</param>
    public void RecordFailedAttempt(string? error, int maxAttempts, TimeSpan retryDelay, TimeProvider timeProvider)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxAttempts);
        ArgumentOutOfRangeException.ThrowIfLessThan(retryDelay, TimeSpan.Zero);
        ArgumentNullException.ThrowIfNull(timeProvider);
        EnsurePending();

        AttemptCount++;
        LastError = error?.Trim() is { Length: > 0 } trimmed
            ? trimmed[..Math.Min(trimmed.Length, NotificationConstants.LastErrorMaxLength)]
            : null;

        if (AttemptCount >= maxAttempts)
        {
            Status = NotificationStatus.Failed;
            NextAttemptAt = null;
            AddDomainEvent(new NotificationFailed(Id, RecipientId, Channel, AttemptCount));

            return;
        }

        NextAttemptAt = Now(timeProvider).Add(retryDelay);
    }

    private static DateTime Now(TimeProvider timeProvider) => timeProvider.GetLocalNow().DateTime;

    private void EnsurePending()
    {
        if (Status != NotificationStatus.Pending)
        {
            throw new ErrorException(ErrorCode.Conflict, NotificationsMessageKeys.NotificationNotPending, Id, Status);
        }
    }
}
