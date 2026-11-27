using Domain.Shared.Layer.Repositories;

namespace OrderFlow.Domain.Orders;

/// <summary>The repository of <see cref="Order"/>. Implemented in EntityFrameworkCore.</summary>
public interface IOrderRepository : IRepository<Order>;
