using Common.Contracts.Enums;
using Common.Domain;

namespace Menu.Domain.Events;

public record RestaurantStatusChangedDomainEvent(Guid RestaurantId, RestaurantStatus NewStatus) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
