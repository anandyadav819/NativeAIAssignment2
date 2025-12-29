using Common.Domain;

namespace Menu.Domain.Events;

public record MenuItemRemovedDomainEvent(Guid RestaurantId, Guid MenuItemId) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
