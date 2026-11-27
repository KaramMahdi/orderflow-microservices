using Domain.Shared.Layer.Errors;
using Domain.Shared.Layer.ValueObjects;
using OrderFlow.Domain.Customers;
using OrderFlow.Domain.Customers.Events;
using OrderFlow.Domain.Shared.Customers;
using OrderFlow.Domain.Tests.TestSupport;

namespace OrderFlow.Domain.Tests.Customers;

/// <summary>Customer, Email and the uniqueness rules.</summary>
public sealed class CustomerTests
{
    private static readonly Address HomeAddress = Address.Create("1 Main St", "Amman", null, "JO");

    [Fact]
    public void EmailIsTrimmedAndLowerCase()
    {
        Assert.Equal("karam@example.com", Email.Create(" Karam@Example.COM ").Value);
    }

    [Theory]
    [InlineData("no-at-sign")]
    [InlineData("Name <a@example.com>")]
    [InlineData("a@")]
    public void EmailRejectsInvalidAddresses(string value)
    {
        ErrorAssert.Throws(ErrorCode.ValidationFailed, CustomersMessageKeys.EmailInvalid, () => Email.Create(value));
    }

    [Fact]
    public void RegisterRaisesCustomerRegistered()
    {
        var userId = Guid.CreateVersion7();

        var customer = Customer.Register(userId, Email.Create("a@example.com"), HomeAddress);

        Assert.Equal(new CustomerRegistered(customer.Id, userId, "a@example.com"), Assert.Single(customer.DomainEvents));
    }

    [Fact]
    public async Task ManagerRejectsATakenEmailOrUser()
    {
        var existing = Customer.Register(Guid.CreateVersion7(), Email.Create("a@example.com"), HomeAddress);
        var manager = new CustomerManager(new CustomerRepository([existing]), new InMemoryQueryableExecuter());
        var cancellationToken = TestContext.Current.CancellationToken;

        await ErrorAssert.ThrowsAsync(ErrorCode.Conflict, CustomersMessageKeys.EmailAlreadyRegistered, () => manager.RegisterAsync(Guid.CreateVersion7(), "A@example.com", HomeAddress, cancellationToken));
        await ErrorAssert.ThrowsAsync(ErrorCode.Conflict, CustomersMessageKeys.UserAlreadyRegistered, () => manager.RegisterAsync(existing.UserId, "b@example.com", HomeAddress, cancellationToken));
    }

    private sealed class CustomerRepository(List<Customer> customers) : ICustomerRepository
    {
        public IQueryable<Customer> GetQueryable() => customers.AsQueryable();
    }
}
