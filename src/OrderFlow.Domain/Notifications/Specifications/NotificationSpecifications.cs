using System.Linq.Expressions;
using Domain.Shared.Layer.Specifications;
using OrderFlow.Domain.Shared.Notifications;

namespace OrderFlow.Domain.Notifications.Specifications;

/// <summary>The pending notifications due at a time (sending and retries).</summary>
/// <param name="now">The current server local time.</param>
public sealed class NotificationsDueSpecification(DateTime now) : Specification<Notification>
{
    /// <inheritdoc />
    public override Expression<Func<Notification, bool>> ToExpression() =>
        notification => notification.Status == NotificationStatus.Pending && notification.NextAttemptAt <= now;
}

/// <summary>The notifications of a recipient.</summary>
/// <param name="recipientId">The recipient.</param>
public sealed class NotificationsOfRecipientSpecification(Guid recipientId) : Specification<Notification>
{
    /// <inheritdoc />
    public override Expression<Func<Notification, bool>> ToExpression() => notification => notification.RecipientId == recipientId;
}
