namespace Common.Messaging.Events;

/// <summary>
/// Event published when payment is processed
/// </summary>
public class PaymentProcessedEvent : IntegrationEvent
{
    public override string EventType => nameof(PaymentProcessedEvent);
    
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public decimal Amount { get; init; }
    public bool IsSuccessful { get; init; }
    public string? TransactionId { get; init; }
    public string? FailureReason { get; init; }
}
