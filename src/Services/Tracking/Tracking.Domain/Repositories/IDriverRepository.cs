using Tracking.Domain.Entities;

namespace Tracking.Domain.Repositories;

/// <summary>
/// Repository interface for Driver aggregate
/// </summary>
public interface IDriverRepository
{
    Task<Driver?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Driver>> GetOnlineDriversAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Driver>> GetAvailableDriversAsync(CancellationToken cancellationToken = default);
    Task<Driver?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task AddAsync(Driver driver, CancellationToken cancellationToken = default);
    Task UpdateAsync(Driver driver, CancellationToken cancellationToken = default);
}
