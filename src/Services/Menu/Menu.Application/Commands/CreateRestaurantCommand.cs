using Common.Application.Messaging;
using Common.Contracts.Models;

namespace Menu.Application.Commands;

public record CreateRestaurantCommand(
    string Name,
    string Description,
    string Address,
    string PhoneNumber,
    TimeSpan OpeningTime,
    TimeSpan ClosingTime,
    decimal DeliveryFee,
    decimal MinimumOrderAmount,
    int EstimatedPreparationTimeMinutes) : ICommand<Result<Guid>>;
