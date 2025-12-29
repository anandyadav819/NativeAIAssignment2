using Common.Contracts.Enums;
using Menu.Domain.Entities;
using Menu.Domain.Repositories;
using Menu.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Menu.Infrastructure.Repositories;

public class RestaurantRepository : IRestaurantRepository
{
    private readonly MenuDbContext _context;

    public RestaurantRepository(MenuDbContext context)
    {
        _context = context;
    }

    public async Task<Restaurant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Restaurants
            .Include(r => r.MenuItems)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Restaurant>> GetActiveRestaurantsAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await _context.Restaurants
            .Include(r => r.MenuItems)
            .Where(r => r.Status == RestaurantStatus.Open)
            .OrderBy(r => r.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetActiveRestaurantCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Restaurants
            .CountAsync(r => r.Status == RestaurantStatus.Open, cancellationToken);
    }

    public async Task<IReadOnlyList<Restaurant>> SearchRestaurantsAsync(
        string searchTerm,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await _context.Restaurants
            .Include(r => r.MenuItems)
            .Where(r => r.Status == RestaurantStatus.Open &&
                       (r.Name.Contains(searchTerm) || r.Description.Contains(searchTerm)))
            .OrderBy(r => r.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Restaurant restaurant, CancellationToken cancellationToken = default)
    {
        await _context.Restaurants.AddAsync(restaurant, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Restaurant restaurant, CancellationToken cancellationToken = default)
    {
        _context.Restaurants.Update(restaurant);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
