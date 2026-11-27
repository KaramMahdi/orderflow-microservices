using Domain.Shared.Layer.DomainModel;
using Domain.Shared.Layer.ValueObjects;
using OrderFlow.Domain.Customers.Events;
using OrderFlow.Domain.Shared.Customers;

namespace OrderFlow.Domain.Customers;

/// <summary>
/// A person who buys (glossary); not the Identity user, which it references by id only.
/// Invariants: a valid email and address. Email and user uniqueness span customers and are checked by
/// <see cref="CustomerManager"/>.
/// </summary>
public sealed class Customer : AggregateRoot
{
    private Customer(Guid id, Guid userId, Email email, Address address)
        : base(id)
    {
        UserId = userId;
        Email = email;
        Address = address;
    }

    /// <summary>The Identity user of this customer (one customer per user).</summary>
    public Guid UserId { get; private set; }

    /// <summary>The email address, unique among customers.</summary>
    public Email Email { get; private set; }

    /// <summary>The address.</summary>
    public Address Address { get; private set; }

    /// <summary>Registers a customer. Prefer <see cref="CustomerManager.RegisterAsync"/>. Raises <see cref="CustomerRegistered"/>.</summary>
    /// <param name="userId">The Identity user.</param>
    /// <param name="email">The email address.</param>
    /// <param name="address">The address.</param>
    public static Customer Register(Guid userId, Email email, Address address)
    {
        ArgumentNullException.ThrowIfNull(email);
        ArgumentNullException.ThrowIfNull(address);
        Check.NotEmpty(userId, nameof(UserId), CustomersMessageKeys.FieldRequired);

        var customer = new Customer(Guid.CreateVersion7(), userId, email, address);
        customer.AddDomainEvent(new CustomerRegistered(customer.Id, userId, email.Value));

        return customer;
    }
}
