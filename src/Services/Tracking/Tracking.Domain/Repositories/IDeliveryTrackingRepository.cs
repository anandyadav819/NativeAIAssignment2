using Tracking.Domain.Entities;

namespace Tracking.Domain.Repositories;

/// <summary>
/// Repository interface for DeliveryTracking aggregate
/// </summary>
public interface IDeliveryTrackingRepository
{
    Task<DeliveryTracking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DeliveryTracking?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DeliveryTracking>> GetActiveDeliveriesAsync(CancellationToken cancellationToken = default);
    Task AddAsync(DeliveryTracking tracking, CancellationToken cancellationToken = default);
    Task UpdateAsync(DeliveryTracking tracking, CancellationToken cancellationToken = default);
}
