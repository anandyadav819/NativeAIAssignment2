namespace Common.Messaging.Events;

/// <summary>
/// Event published when order status changes
/// </summary>
public class OrderStatusChangedEvent : IntegrationEvent
{
    public override string EventType => nameof(OrderStatusChangedEvent);
    
    public Guid OrderId { get; init; }
    public string OldStatus { get; init; } = string.Empty;
    public string NewStatus { get; init; } = string.Empty;
    public Guid? DriverId { get; init; }
}
