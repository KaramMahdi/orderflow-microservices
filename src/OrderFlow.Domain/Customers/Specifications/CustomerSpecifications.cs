using System.Linq.Expressions;
using Domain.Shared.Layer.Specifications;

namespace OrderFlow.Domain.Customers.Specifications;

/// <summary>The customer with an email address.</summary>
/// <param name="email">The email address.</param>
public sealed class CustomerByEmailSpecification(Email email) : Specification<Customer>
{
    /// <inheritdoc />
    public override Expression<Func<Customer, bool>> ToExpression() => customer => customer.Email == email;
}

/// <summary>The customer of an Identity user.</summary>
/// <param name="userId">The user.</param>
public sealed class CustomerOfUserSpecification(Guid userId) : Specification<Customer>
{
    /// <inheritdoc />
    public override Expression<Func<Customer, bool>> ToExpression() => customer => customer.UserId == userId;
}
