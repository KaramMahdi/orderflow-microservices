namespace Domain.Shared.Layer.Messaging;

/// <summary>
/// Publishes integration events to other contexts (Phase 3). Implemented once in Shared.Infrastructure
/// (ADR-0017), so no inner layer references a RabbitMQ package. Integration event contracts live in
/// Domain.Shared.Layer.
/// </summary>
/// <remarks>
/// TODO(Phase 3): duplicates (R-011), an event saved but not published (R-012, outbox or not) and
/// ordering (R-014) are Open risks. The event type constraint narrows to the integration event base
/// type when Phase 3 adds the contracts.
/// </remarks>
public interface IMessagePublisher
{
    /// <summary>Publishes one integration event.</summary>
    /// <typeparam name="TEvent">The integration event type.</typeparam>
    /// <param name="integrationEvent">The event to publish.</param>
    /// <param name="cancellationToken">Cancels the publish.</param>
    Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : class;
}
