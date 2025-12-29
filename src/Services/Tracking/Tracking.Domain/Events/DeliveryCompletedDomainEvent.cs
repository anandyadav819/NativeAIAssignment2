using Common.Domain;

namespace Tracking.Domain.Events;

public record DeliveryCompletedDomainEvent(Guid DriverId, Guid OrderId) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
