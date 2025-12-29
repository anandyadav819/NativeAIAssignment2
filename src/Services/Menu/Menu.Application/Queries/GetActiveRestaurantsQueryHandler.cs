using Common.Application.Messaging;
using Common.Contracts.Models;
using Menu.Application.DTOs;
using Menu.Domain.Repositories;

namespace Menu.Application.Queries;

public class GetActiveRestaurantsQueryHandler : IQueryHandler<GetActiveRestaurantsQuery, PagedResult<RestaurantDto>>
{
    private readonly IRestaurantRepository _restaurantRepository;

    public GetActiveRestaurantsQueryHandler(IRestaurantRepository restaurantRepository)
    {
        _restaurantRepository = restaurantRepository;
    }

    public async Task<PagedResult<RestaurantDto>> Handle(GetActiveRestaurantsQuery request, CancellationToken cancellationToken)
    {
        var restaurants = await _restaurantRepository.GetActiveRestaurantsAsync(
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var totalCount = await _restaurantRepository.GetActiveRestaurantCountAsync(cancellationToken);

        var restaurantDtos = restaurants.Select(r => new RestaurantDto
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description,
            Address = r.Address,
            PhoneNumber = r.PhoneNumber,
            Status = r.Status,
            OpeningTime = r.OpeningTime,
            ClosingTime = r.ClosingTime,
            DeliveryFee = r.DeliveryFee,
            MinimumOrderAmount = r.MinimumOrderAmount,
            EstimatedPreparationTimeMinutes = r.EstimatedPreparationTimeMinutes,
            CreatedAt = r.CreatedAt,
            MenuItems = r.MenuItems.Select(m => new MenuItemDto
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
        }).ToList();

        return new PagedResult<RestaurantDto>(restaurantDtos, totalCount, request.PageNumber, request.PageSize);
    }
}
