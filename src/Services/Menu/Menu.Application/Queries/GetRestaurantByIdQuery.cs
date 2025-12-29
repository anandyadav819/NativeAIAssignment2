using Common.Application.Messaging;
using Menu.Application.DTOs;

namespace Menu.Application.Queries;

public record GetRestaurantByIdQuery(Guid RestaurantId) : IQuery<RestaurantDto?>;
