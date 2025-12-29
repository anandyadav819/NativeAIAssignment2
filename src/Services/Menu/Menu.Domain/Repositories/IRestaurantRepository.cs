using Menu.Domain.Entities;

namespace Menu.Domain.Repositories;

/// <summary>
/// Repository interface for Restaurant aggregate
/// </summary>
public interface IRestaurantRepository
{
    Task<Restaurant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Restaurant>> GetActiveRestaurantsAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetActiveRestaurantCountAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Restaurant>> SearchRestaurantsAsync(string searchTerm, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task AddAsync(Restaurant restaurant, CancellationToken cancellationToken = default);
    Task UpdateAsync(Restaurant restaurant, CancellationToken cancellationToken = default);
}
