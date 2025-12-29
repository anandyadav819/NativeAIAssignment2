using Common.Application.Messaging;
using Common.Contracts.Models;
using Menu.Application.DTOs;

namespace Menu.Application.Queries;

public record GetActiveRestaurantsQuery(
    int PageNumber = 1,
    int PageSize = 20) : IQuery<PagedResult<RestaurantDto>>;
