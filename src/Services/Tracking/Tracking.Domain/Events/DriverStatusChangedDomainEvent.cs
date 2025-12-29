using Common.Contracts.Enums;
using Common.Domain;

namespace Tracking.Domain.Events;

public record DriverStatusChangedDomainEvent(Guid DriverId, DriverStatus NewStatus) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
