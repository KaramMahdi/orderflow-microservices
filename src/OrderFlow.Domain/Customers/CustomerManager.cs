using Domain.Shared.Layer.Errors;
using Domain.Shared.Layer.Repositories;
using Domain.Shared.Layer.ValueObjects;
using OrderFlow.Domain.Customers.Specifications;
using OrderFlow.Domain.Shared.Customers;

namespace OrderFlow.Domain.Customers;

/// <summary>
/// Registers customers with an email and a user no other customer has (rules across customers).
/// The unique indexes are the safety net for concurrent registrations. It does not save.
/// </summary>
/// <param name="customerRepository">The customer repository.</param>
/// <param name="queryableExecuter">Executes the queries.</param>
public sealed class CustomerManager(ICustomerRepository customerRepository, IAsyncQueryableExecuter queryableExecuter)
{
    /// <summary>Registers a customer after the uniqueness checks.</summary>
    /// <param name="userId">The Identity user.</param>
    /// <param name="email">The email address.</param>
    /// <param name="address">The address.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task<Customer> RegisterAsync(Guid userId, string? email, Address address, CancellationToken cancellationToken = default)
    {
        var validEmail = Email.Create(email);

        var byEmail = customerRepository.GetQueryable().Where(new CustomerByEmailSpecification(validEmail).ToExpression());

        if (await queryableExecuter.CountAsync(byEmail, cancellationToken).ConfigureAwait(false) > 0)
        {
            throw new ErrorException(ErrorCode.Conflict, CustomersMessageKeys.EmailAlreadyRegistered, validEmail.Value);
        }

        var byUser = customerRepository.GetQueryable().Where(new CustomerOfUserSpecification(userId).ToExpression());

        if (await queryableExecuter.CountAsync(byUser, cancellationToken).ConfigureAwait(false) > 0)
        {
            throw new ErrorException(ErrorCode.Conflict, CustomersMessageKeys.UserAlreadyRegistered, userId);
        }

        return Customer.Register(userId, validEmail, address);
    }
}
