using Menu.Domain.Entities;

namespace Menu.Domain.Repositories;

/// <summary>
/// Repository interface for MenuItem queries
/// </summary>
public interface IMenuItemRepository
{
    Task<MenuItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MenuItem>> GetByRestaurantIdAsync(Guid restaurantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MenuItem>> GetByCategoryAsync(Guid restaurantId, string category, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MenuItem>> SearchAsync(string searchTerm, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
}
