using Domain.Shared.Layer.Repositories;

namespace OrderFlow.Domain.Notifications;

/// <summary>The repository of <see cref="Notification"/>. Implemented in EntityFrameworkCore.</summary>
public interface INotificationRepository : IRepository<Notification>;
