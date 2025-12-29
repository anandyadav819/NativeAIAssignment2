using Common.Application.Messaging;
using Common.Contracts.Models;
using Menu.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Menu.Application.Commands;

public class AddMenuItemCommandHandler : ICommandHandler<AddMenuItemCommand, Result<Guid>>
{
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly ILogger<AddMenuItemCommandHandler> _logger;

    public AddMenuItemCommandHandler(
        IRestaurantRepository restaurantRepository,
        ILogger<AddMenuItemCommandHandler> logger)
    {
        _restaurantRepository = restaurantRepository;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(AddMenuItemCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var restaurant = await _restaurantRepository.GetByIdAsync(request.RestaurantId, cancellationToken);
            if (restaurant == null)
            {
                return Result.Failure<Guid>($"Restaurant {request.RestaurantId} not found");
            }

            var menuItem = restaurant.AddMenuItem(
                request.Name,
                request.Description,
                request.Price,
                request.Category,
                request.ImageUrl);

            await _restaurantRepository.UpdateAsync(restaurant, cancellationToken);

            _logger.LogInformation("Menu item {MenuItemId} added to restaurant {RestaurantId}", 
                menuItem.Id, restaurant.Id);

            return Result.Success(menuItem.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding menu item to restaurant {RestaurantId}", request.RestaurantId);
            return Result.Failure<Guid>($"Failed to add menu item: {ex.Message}");
        }
    }
}
