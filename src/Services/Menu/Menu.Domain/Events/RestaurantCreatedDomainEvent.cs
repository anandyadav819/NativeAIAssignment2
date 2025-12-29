using Common.Domain;

namespace Menu.Domain.Events;

public record RestaurantCreatedDomainEvent(Guid RestaurantId, string Name) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
