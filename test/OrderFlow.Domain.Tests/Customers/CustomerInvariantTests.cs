using Domain.Shared.Layer.Errors;
using Domain.Shared.Layer.ValueObjects;
using OrderFlow.Domain.Customers;
using OrderFlow.Domain.Customers.Specifications;
using OrderFlow.Domain.Shared.Customers;
using OrderFlow.Domain.Tests.TestSupport;

namespace OrderFlow.Domain.Tests.Customers;

/// <summary>The remaining Customer invariants.</summary>
public sealed class CustomerInvariantTests
{
    private static readonly Address HomeAddress = Address.Create("1 Main St", "Amman", null, "JO");

    [Theory]
    [InlineData(null)]
    [InlineData("  ")]
    public void EmailIsRequired(string? value)
    {
        ErrorAssert.Throws(ErrorCode.ValidationFailed, CustomersMessageKeys.FieldRequired, () => Email.Create(value));
    }

    [Fact]
    public void EmailHasAMaximumLength()
    {
        var tooLong = new string('a', CustomerConstants.EmailMaxLength) + "@example.com";

        ErrorAssert.Throws(ErrorCode.ValidationFailed, CustomersMessageKeys.FieldTooLong, () => Email.Create(tooLong));
    }

    [Fact]
    public void EmailComparesWithoutCase()
    {
        Assert.Equal(Email.Create("A@Example.com"), Email.Create("a@example.COM"));
    }

    [Fact]
    public void ACustomerNeedsAUser()
    {
        ErrorAssert.Throws(ErrorCode.ValidationFailed, CustomersMessageKeys.FieldRequired, () => Customer.Register(Guid.Empty, Email.Create("a@example.com"), HomeAddress));
    }

    [Fact]
    public async Task ManagerRegistersAFreeEmailAndUser()
    {
        var manager = new CustomerManager(new CustomerRepository([]), new InMemoryQueryableExecuter());
        var userId = Guid.CreateVersion7();

        var customer = await manager.RegisterAsync(userId, "New@Example.com", HomeAddress, TestContext.Current.CancellationToken);

        Assert.Equal(userId, customer.UserId);
        Assert.Equal("new@example.com", customer.Email.Value);
        Assert.Equal(HomeAddress, customer.Address);
        Assert.True(new CustomerByEmailSpecification(Email.Create("NEW@example.com")).IsSatisfiedBy(customer));
        Assert.True(new CustomerOfUserSpecification(userId).IsSatisfiedBy(customer));
    }

    [Fact]
    public async Task ManagerValidatesTheEmailBeforeQuerying()
    {
        var manager = new CustomerManager(new CustomerRepository([]), new InMemoryQueryableExecuter());

        await ErrorAssert.ThrowsAsync(ErrorCode.ValidationFailed, CustomersMessageKeys.EmailInvalid, () => manager.RegisterAsync(Guid.CreateVersion7(), "bad", HomeAddress, TestContext.Current.CancellationToken));
    }

    private sealed class CustomerRepository(List<Customer> customers) : ICustomerRepository
    {
        public IQueryable<Customer> GetQueryable() => customers.AsQueryable();
    }
}
