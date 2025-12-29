using Common.Application.Messaging;
using Common.Contracts.Models;

namespace Order.Application.Commands;

public record CreateOrderCommand(
    Guid CustomerId,
    Guid RestaurantId,
    string DeliveryAddress,
    List<OrderItemCommand> Items) : ICommand<Result<Guid>>;

public record OrderItemCommand(
    Guid MenuItemId,
    string Name,
    int Quantity,
    decimal UnitPrice);
