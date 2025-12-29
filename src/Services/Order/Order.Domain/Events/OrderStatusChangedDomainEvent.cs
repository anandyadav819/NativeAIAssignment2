using Common.Contracts.Enums;
using Common.Domain;

namespace Order.Domain.Events;

public record OrderStatusChangedDomainEvent(
    Guid OrderId,
    OrderStatus OldStatus,
    OrderStatus NewStatus) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
