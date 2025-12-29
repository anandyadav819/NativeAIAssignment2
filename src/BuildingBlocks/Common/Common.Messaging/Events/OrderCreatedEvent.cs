namespace Common.Messaging.Events;

/// <summary>
/// Event published when an order is created
/// </summary>
public class OrderCreatedEvent : IntegrationEvent
{
    public override string EventType => nameof(OrderCreatedEvent);
    
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public Guid RestaurantId { get; init; }
    public decimal TotalAmount { get; init; }
    public List<OrderItemDto> Items { get; init; } = new();
}

public class OrderItemDto
{
    public Guid MenuItemId { get; init; }
    public string Name { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
}
