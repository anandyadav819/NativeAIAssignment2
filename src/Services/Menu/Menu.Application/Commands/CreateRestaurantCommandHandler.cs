using Common.Application.Messaging;
using Common.Contracts.Models;
using Menu.Domain.Entities;
using Menu.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Menu.Application.Commands;

public class CreateRestaurantCommandHandler : ICommandHandler<CreateRestaurantCommand, Result<Guid>>
{
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly ILogger<CreateRestaurantCommandHandler> _logger;

    public CreateRestaurantCommandHandler(
        IRestaurantRepository restaurantRepository,
        ILogger<CreateRestaurantCommandHandler> logger)
    {
        _restaurantRepository = restaurantRepository;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateRestaurantCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var restaurant = new Restaurant(
                request.Name,
                request.Description,
                request.Address,
                request.PhoneNumber,
                request.OpeningTime,
                request.ClosingTime,
                request.DeliveryFee,
                request.MinimumOrderAmount,
                request.EstimatedPreparationTimeMinutes);

            await _restaurantRepository.AddAsync(restaurant, cancellationToken);

            _logger.LogInformation("Restaurant {RestaurantId} created: {Name}", restaurant.Id, restaurant.Name);

            return Result.Success(restaurant.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating restaurant {Name}", request.Name);
            return Result.Failure<Guid>($"Failed to create restaurant: {ex.Message}");
        }
    }
}
