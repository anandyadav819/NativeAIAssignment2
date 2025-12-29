using Menu.Domain.Entities;
using Menu.Domain.Repositories;
using Menu.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Menu.Infrastructure.Repositories;

public class MenuItemRepository : IMenuItemRepository
{
    private readonly MenuDbContext _context;

    public MenuItemRepository(MenuDbContext context)
    {
        _context = context;
    }

    public async Task<MenuItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Menu items are owned by Restaurant, so we need to query through Restaurant
        var restaurant = await _context.Restaurants
            .Include(r => r.MenuItems)
            .FirstOrDefaultAsync(r => r.MenuItems.Any(m => m.Id == id), cancellationToken);

        return restaurant?.MenuItems.FirstOrDefault(m => m.Id == id);
    }

    public async Task<IReadOnlyList<MenuItem>> GetByRestaurantIdAsync(
        Guid restaurantId,
        CancellationToken cancellationToken = default)
    {
        var restaurant = await _context.Restaurants
            .Include(r => r.MenuItems)
            .FirstOrDefaultAsync(r => r.Id == restaurantId, cancellationToken);

        return restaurant?.MenuItems.ToList() ?? new List<MenuItem>();
    }

    public async Task<IReadOnlyList<MenuItem>> GetByCategoryAsync(
        Guid restaurantId,
        string category,
        CancellationToken cancellationToken = default)
    {
        var restaurant = await _context.Restaurants
            .Include(r => r.MenuItems)
            .FirstOrDefaultAsync(r => r.Id == restaurantId, cancellationToken);

        return restaurant?.MenuItems.Where(m => m.Category == category).ToList() ?? new List<MenuItem>();
    }

    public async Task<IReadOnlyList<MenuItem>> SearchAsync(
        string searchTerm,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var restaurants = await _context.Restaurants
            .Include(r => r.MenuItems)
            .Where(r => r.MenuItems.Any(m => m.Name.Contains(searchTerm) || m.Description.Contains(searchTerm)))
            .ToListAsync(cancellationToken);

        var allMenuItems = restaurants
            .SelectMany(r => r.MenuItems)
            .Where(m => m.Name.Contains(searchTerm) || m.Description.Contains(searchTerm))
            .OrderBy(m => m.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return allMenuItems;
    }
}
