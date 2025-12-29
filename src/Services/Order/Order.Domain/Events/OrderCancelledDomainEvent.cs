using Common.Contracts.Enums;
using Common.Domain;

namespace Order.Domain.Events;

public record OrderCancelledDomainEvent(
    Guid OrderId,
    string Reason,
    OrderStatus PreviousStatus) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
