using Common.Domain;

namespace Tracking.Domain.Events;

public record DriverLocationUpdatedDomainEvent(
    Guid DriverId,
    double Latitude,
    double Longitude,
    Guid? OrderId) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
