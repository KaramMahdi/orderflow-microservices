using Domain.Shared.Layer.Repositories;

namespace OrderFlow.Domain.Customers;

/// <summary>The repository of <see cref="Customer"/>. Implemented in EntityFrameworkCore.</summary>
public interface ICustomerRepository : IRepository<Customer>;
