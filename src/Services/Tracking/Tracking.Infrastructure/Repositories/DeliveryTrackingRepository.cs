using Microsoft.EntityFrameworkCore;
using Tracking.Domain.Entities;
using Tracking.Domain.Repositories;
using Tracking.Infrastructure.Persistence;

namespace Tracking.Infrastructure.Repositories;

public class DeliveryTrackingRepository : IDeliveryTrackingRepository
{
    private readonly TrackingDbContext _context;

    public DeliveryTrackingRepository(TrackingDbContext context)
    {
        _context = context;
    }

    public async Task<DeliveryTracking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.DeliveryTrackings
            .Include(dt => dt.LocationHistory)
            .FirstOrDefaultAsync(dt => dt.Id == id, cancellationToken);
    }

    public async Task<DeliveryTracking?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _context.DeliveryTrackings
            .Include(dt => dt.LocationHistory)
            .FirstOrDefaultAsync(dt => dt.OrderId == orderId, cancellationToken);
    }

    public async Task<IReadOnlyList<DeliveryTracking>> GetActiveDeliveriesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.DeliveryTrackings
            .Include(dt => dt.LocationHistory)
            .Where(dt => dt.DeliveredAt == null)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(DeliveryTracking tracking, CancellationToken cancellationToken = default)
    {
        await _context.DeliveryTrackings.AddAsync(tracking, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(DeliveryTracking tracking, CancellationToken cancellationToken = default)
    {
        _context.DeliveryTrackings.Update(tracking);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
