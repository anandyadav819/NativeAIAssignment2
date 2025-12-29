using Common.Contracts.Enums;
using Microsoft.EntityFrameworkCore;
using Tracking.Domain.Entities;
using Tracking.Domain.Repositories;
using Tracking.Infrastructure.Persistence;

namespace Tracking.Infrastructure.Repositories;

public class DriverRepository : IDriverRepository
{
    private readonly TrackingDbContext _context;

    public DriverRepository(TrackingDbContext context)
    {
        _context = context;
    }

    public async Task<Driver?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Drivers
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Driver>> GetOnlineDriversAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Drivers
            .Where(d => d.Status == DriverStatus.Available || d.Status == DriverStatus.Busy)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Driver>> GetAvailableDriversAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Drivers
            .Where(d => d.Status == DriverStatus.Available)
            .ToListAsync(cancellationToken);
    }

    public async Task<Driver?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _context.Drivers
            .FirstOrDefaultAsync(d => d.CurrentOrderId == orderId, cancellationToken);
    }

    public async Task AddAsync(Driver driver, CancellationToken cancellationToken = default)
    {
        await _context.Drivers.AddAsync(driver, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Driver driver, CancellationToken cancellationToken = default)
    {
        _context.Drivers.Update(driver);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
