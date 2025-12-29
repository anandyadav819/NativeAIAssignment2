namespace Order.Domain.Repositories;

/// <summary>
/// Repository interface for Order aggregate
/// </summary>
public interface IOrderRepository
{
    Task<Entities.Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Entities.Order>> GetByCustomerIdAsync(Guid customerId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetCustomerOrderCountAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task AddAsync(Entities.Order order, CancellationToken cancellationToken = default);
    Task UpdateAsync(Entities.Order order, CancellationToken cancellationToken = default);
}
