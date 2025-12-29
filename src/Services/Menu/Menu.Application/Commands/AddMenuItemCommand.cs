using Common.Application.Messaging;
using Common.Contracts.Models;

namespace Menu.Application.Commands;

public record AddMenuItemCommand(
    Guid RestaurantId,
    string Name,
    string Description,
    decimal Price,
    string Category,
    string? ImageUrl = null) : ICommand<Result<Guid>>;
