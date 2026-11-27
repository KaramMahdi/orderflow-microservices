using Domain.Shared.Layer.Repositories;

namespace OrderFlow.Domain.Payments;

/// <summary>The repository of <see cref="Payment"/>. Implemented in EntityFrameworkCore.</summary>
public interface IPaymentRepository : IRepository<Payment>;
