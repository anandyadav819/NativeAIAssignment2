using Common.Application.Messaging;
using Common.Caching;
using Menu.Application.DTOs;
using Menu.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Menu.Application.Queries;

public class GetRestaurantByIdQueryHandler : IQueryHandler<GetRestaurantByIdQuery, RestaurantDto?>
{
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<GetRestaurantByIdQueryHandler> _logger;

    public GetRestaurantByIdQueryHandler(
        IRestaurantRepository restaurantRepository,
        ICacheService cacheService,
        ILogger<GetRestaurantByIdQueryHandler> logger)
    {
        _restaurantRepository = restaurantRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<RestaurantDto?> Handle(GetRestaurantByIdQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"restaurant:{request.RestaurantId}";

        var restaurantDto = await _cacheService.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                var restaurant = await _restaurantRepository.GetByIdAsync(request.RestaurantId, cancellationToken);
                if (restaurant == null)
                {
                    return null;
                }

                return new RestaurantDto
                {
                    Id = restaurant.Id,
                    Name = restaurant.Name,
                    Description = restaurant.Description,
                    Address = restaurant.Address,
                    PhoneNumber = restaurant.PhoneNumber,
                    Status = restaurant.Status,
                    OpeningTime = restaurant.OpeningTime,
                    ClosingTime = restaurant.ClosingTime,
                    DeliveryFee = restaurant.DeliveryFee,
                    MinimumOrderAmount = restaurant.MinimumOrderAmount,
                    EstimatedPreparationTimeMinutes = restaurant.EstimatedPreparationTimeMinutes,
                    CreatedAt = restaurant.CreatedAt,
                    MenuItems = restaurant.MenuItems.Select(m => new MenuItemDto
                    {
                        Id = m.Id,
                        RestaurantId = m.RestaurantId,
                        Name = m.Name,
                        Description = m.Description,
                        Price = m.Price,
                        Category = m.Category,
                        ImageUrl = m.ImageUrl,
                        IsAvailable = m.IsAvailable
                    }).ToList()
                };
            },
            TimeSpan.FromMinutes(10));

        return restaurantDto;
    }
}
