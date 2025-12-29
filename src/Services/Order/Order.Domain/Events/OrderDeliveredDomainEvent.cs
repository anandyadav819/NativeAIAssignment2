using Common.Domain;

namespace Order.Domain.Events;

public record OrderDeliveredDomainEvent(Guid OrderId) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
