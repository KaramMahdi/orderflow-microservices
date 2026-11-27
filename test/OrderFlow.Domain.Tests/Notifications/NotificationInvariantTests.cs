using Domain.Shared.Layer.Errors;
using OrderFlow.Domain.Notifications;
using OrderFlow.Domain.Notifications.Specifications;
using OrderFlow.Domain.Shared.Notifications;
using OrderFlow.Domain.Tests.TestSupport;

namespace OrderFlow.Domain.Tests.Notifications;

/// <summary>The remaining Notification invariants.</summary>
public sealed class NotificationInvariantTests
{
    private const string Address = "a@example.com";
    private const string Body = "Your order was placed.";

    private readonly FixedTimeProvider _clock = new(new DateTimeOffset(2026, 11, 27, 10, 0, 0, TimeSpan.Zero));

    [Fact]
    public void ANotificationNeedsARecipient()
    {
        ErrorAssert.Throws(ErrorCode.ValidationFailed, NotificationsMessageKeys.FieldRequired, () => Create(Guid.Empty, Address, null, Body));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(" ")]
    public void ANotificationNeedsAnAddress(string? address)
    {
        ErrorAssert.Throws(ErrorCode.ValidationFailed, NotificationsMessageKeys.FieldRequired, () => Create(Guid.CreateVersion7(), address, null, Body));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ANotificationNeedsABody(string? body)
    {
        ErrorAssert.Throws(ErrorCode.ValidationFailed, NotificationsMessageKeys.FieldRequired, () => Create(Guid.CreateVersion7(), Address, null, body));
    }

    [Fact]
    public void TextsHaveMaximumLengths()
    {
        var recipient = Guid.CreateVersion7();

        ErrorAssert.Throws(ErrorCode.ValidationFailed, NotificationsMessageKeys.FieldTooLong, () => Create(recipient, new string('a', NotificationConstants.RecipientAddressMaxLength + 1), null, Body));
        ErrorAssert.Throws(ErrorCode.ValidationFailed, NotificationsMessageKeys.FieldTooLong, () => Create(recipient, Address, new string('s', NotificationConstants.SubjectMaxLength + 1), Body));
        ErrorAssert.Throws(ErrorCode.ValidationFailed, NotificationsMessageKeys.FieldTooLong, () => Create(recipient, Address, null, new string('b', NotificationConstants.BodyMaxLength + 1)));
    }

    [Fact]
    public void ASmsHasNoSubject()
    {
        var notification = Notification.Create(Guid.CreateVersion7(), NotificationChannel.Sms, "+962790000000", " ", Body, _clock);

        Assert.Null(notification.Subject);
        Assert.Equal(NotificationChannel.Sms, notification.Channel);
        Assert.Equal(0, notification.AttemptCount);
    }

    [Fact]
    public void TheMaximumAndTheDelayFromOptionsMustBeSensible()
    {
        var notification = Create(Guid.CreateVersion7(), Address, null, Body);

        Assert.Throws<ArgumentOutOfRangeException>(() => notification.RecordFailedAttempt("e", 0, TimeSpan.Zero, _clock));
        Assert.Throws<ArgumentOutOfRangeException>(() => notification.RecordFailedAttempt("e", 1, TimeSpan.FromSeconds(-1), _clock));
        Assert.Equal(0, notification.AttemptCount);
    }

    [Fact]
    public void AFailedNotificationCannotBeSent()
    {
        var notification = Create(Guid.CreateVersion7(), Address, null, Body);
        notification.RecordFailedAttempt("e", 1, TimeSpan.Zero, _clock);

        ErrorAssert.Throws(ErrorCode.Conflict, NotificationsMessageKeys.NotificationNotPending, notification.MarkSent);
    }

    [Fact]
    public void ARetriedNotificationBecomesDueAfterTheDelay()
    {
        var notification = Create(Guid.CreateVersion7(), Address, null, Body);
        notification.RecordFailedAttempt("e", 3, TimeSpan.FromMinutes(5), _clock);

        _clock.Now = _clock.Now.AddMinutes(5);

        Assert.True(new NotificationsDueSpecification(_clock.Now.DateTime).IsSatisfiedBy(notification));
        notification.MarkSent();
        Assert.Equal(2, notification.AttemptCount);
        Assert.False(new NotificationsDueSpecification(_clock.Now.DateTime).IsSatisfiedBy(notification));
    }

    [Fact]
    public void RecipientSpecificationSelectsTheRecipient()
    {
        var recipient = Guid.CreateVersion7();

        Assert.True(new NotificationsOfRecipientSpecification(recipient).IsSatisfiedBy(Create(recipient, Address, null, Body)));
    }

    private Notification Create(Guid recipientId, string? address, string? subject, string? body) =>
        Notification.Create(recipientId, NotificationChannel.Email, address, subject, body, _clock);
}
