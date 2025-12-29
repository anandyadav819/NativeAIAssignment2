using Common.Domain;

namespace Tracking.Domain.Events;

public record DriverCreatedDomainEvent(Guid DriverId, string Name) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
