using Domain.Shared.Layer.DomainModel;

namespace OrderFlow.Domain.Customers.Events;

/// <summary>A customer was registered.</summary>
/// <param name="CustomerId">The customer.</param>
/// <param name="UserId">The Identity user.</param>
/// <param name="Email">The email address.</param>
public sealed record CustomerRegistered(Guid CustomerId, Guid UserId, string Email) : IDomainEvent;
