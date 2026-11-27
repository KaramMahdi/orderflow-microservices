using Domain.Shared.Layer.Errors;
using OrderFlow.Domain.Notifications;
using OrderFlow.Domain.Notifications.Events;
using OrderFlow.Domain.Notifications.Specifications;
using OrderFlow.Domain.Shared.Notifications;
using OrderFlow.Domain.Tests.TestSupport;

namespace OrderFlow.Domain.Tests.Notifications;

/// <summary>Notification attempts and retries.</summary>
public sealed class NotificationTests
{
    private const int MaxAttempts = 3;

    private static readonly TimeSpan RetryDelay = TimeSpan.FromMinutes(5);

    private readonly FixedTimeProvider _clock = new(new DateTimeOffset(2026, 11, 27, 10, 0, 0, TimeSpan.Zero));

    [Fact]
    public void ANewNotificationIsDueNow()
    {
        var notification = Create();

        Assert.Equal(NotificationStatus.Pending, notification.Status);
        Assert.True(new NotificationsDueSpecification(_clock.Now.DateTime).IsSatisfiedBy(notification));
    }

    [Fact]
    public void AFailedTryIsScheduledAgainBelowTheMaximum()
    {
        var notification = Create();

        notification.RecordFailedAttempt("smtp timeout", MaxAttempts, RetryDelay, _clock);

        Assert.Equal(NotificationStatus.Pending, notification.Status);
        Assert.Equal(1, notification.AttemptCount);
        Assert.Equal(_clock.Now.DateTime + RetryDelay, notification.NextAttemptAt);
        Assert.False(new NotificationsDueSpecification(_clock.Now.DateTime).IsSatisfiedBy(notification));
    }

    [Fact]
    public void AttemptsNeverGoAboveTheMaximum()
    {
        var notification = Create();

        for (var attempt = 0; attempt < MaxAttempts; attempt++)
        {
            notification.RecordFailedAttempt("smtp timeout", MaxAttempts, RetryDelay, _clock);
        }

        Assert.Equal(NotificationStatus.Failed, notification.Status);
        Assert.Equal(MaxAttempts, notification.AttemptCount);
        Assert.Null(notification.NextAttemptAt);
        Assert.IsType<NotificationFailed>(Assert.Single(notification.DomainEvents));
        ErrorAssert.Throws(ErrorCode.Conflict, NotificationsMessageKeys.NotificationNotPending, () => notification.RecordFailedAttempt("again", MaxAttempts, RetryDelay, _clock));
    }

    [Fact]
    public void ASentNotificationIsFinal()
    {
        var notification = Create();

        notification.MarkSent();

        Assert.Equal(NotificationStatus.Sent, notification.Status);
        Assert.IsType<NotificationSent>(Assert.Single(notification.DomainEvents));
        ErrorAssert.Throws(ErrorCode.Conflict, NotificationsMessageKeys.NotificationNotPending, notification.MarkSent);
    }

    [Fact]
    public void AnUnknownChannelIsRejected()
    {
        ErrorAssert.Throws(
            ErrorCode.ValidationFailed,
            NotificationsMessageKeys.ChannelInvalid,
            () => Notification.Create(Guid.CreateVersion7(), (NotificationChannel)99, "a@example.com", null, "Hello", _clock));
    }

    [Fact]
    public void TheLastErrorIsCut()
    {
        var notification = Create();

        notification.RecordFailedAttempt(new string('x', NotificationConstants.LastErrorMaxLength + 10), MaxAttempts, RetryDelay, _clock);

        Assert.Equal(NotificationConstants.LastErrorMaxLength, notification.LastError!.Length);
    }

    private Notification Create() =>
        Notification.Create(Guid.CreateVersion7(), NotificationChannel.Email, "a@example.com", "Order placed", "Your order was placed.", _clock);
}
