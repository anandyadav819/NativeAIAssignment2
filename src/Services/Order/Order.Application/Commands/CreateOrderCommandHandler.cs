using Common.Application.Messaging;
using Common.Contracts.Models;
using Common.EventBus;
using Common.Messaging.Events;
using Microsoft.Extensions.Logging;
using Order.Domain.Repositories;

namespace Order.Application.Commands;

public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, Result<Guid>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IEventBus _eventBus;
    private readonly ILogger<CreateOrderCommandHandler> _logger;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        IEventBus eventBus,
        ILogger<CreateOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Create order aggregate
            var order = new Domain.Entities.Order(
                request.CustomerId,
                request.RestaurantId,
                request.DeliveryAddress);

            // Add items
            foreach (var item in request.Items)
            {
                order.AddItem(item.MenuItemId, item.Name, item.Quantity, item.UnitPrice);
            }

            // Save to repository
            await _orderRepository.AddAsync(order, cancellationToken);

            _logger.LogInformation("Order {OrderId} created for customer {CustomerId}", 
                order.Id, order.CustomerId);

            // Publish integration event
            var integrationEvent = new OrderCreatedEvent
            {
                OrderId = order.Id,
                CustomerId = order.CustomerId,
                RestaurantId = order.RestaurantId,
                TotalAmount = order.TotalAmount,
                Items = order.Items.Select(i => new OrderItemDto
                {
                    MenuItemId = i.MenuItemId,
                    Name = i.Name,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };

            await _eventBus.PublishAsync(integrationEvent, cancellationToken);

            return Result.Success(order.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order for customer {CustomerId}", request.CustomerId);
            return Result.Failure<Guid>($"Failed to create order: {ex.Message}");
        }
    }
}
