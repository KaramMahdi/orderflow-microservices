using Domain.Shared.Layer.DomainModel;
using OrderFlow.Domain.Shared.Notifications;

namespace OrderFlow.Domain.Notifications.Events;

/// <summary>A notification was sent.</summary>
/// <param name="NotificationId">The notification.</param>
/// <param name="RecipientId">The recipient.</param>
/// <param name="Channel">The channel.</param>
public sealed record NotificationSent(Guid NotificationId, Guid RecipientId, NotificationChannel Channel) : IDomainEvent;

/// <summary>Every allowed try of a notification failed.</summary>
/// <param name="NotificationId">The notification.</param>
/// <param name="RecipientId">The recipient.</param>
/// <param name="Channel">The channel.</param>
/// <param name="AttemptCount">The number of tries.</param>
public sealed record NotificationFailed(Guid NotificationId, Guid RecipientId, NotificationChannel Channel, int AttemptCount) : IDomainEvent;
