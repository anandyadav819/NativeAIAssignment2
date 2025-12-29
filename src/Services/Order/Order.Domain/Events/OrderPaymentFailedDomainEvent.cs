using Common.Domain;

namespace Order.Domain.Events;

public record OrderPaymentFailedDomainEvent(
    Guid OrderId,
    string Reason) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
