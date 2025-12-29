using Common.Domain;

namespace Order.Domain.Events;

public record OrderAssignedToDriverDomainEvent(
    Guid OrderId,
    Guid DriverId) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
