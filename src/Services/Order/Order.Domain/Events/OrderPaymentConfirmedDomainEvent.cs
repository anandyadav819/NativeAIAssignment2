using Common.Domain;

namespace Order.Domain.Events;

public record OrderPaymentConfirmedDomainEvent(
    Guid OrderId,
    string TransactionId) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
