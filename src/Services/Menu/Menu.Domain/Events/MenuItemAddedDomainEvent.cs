using Common.Domain;

namespace Menu.Domain.Events;

public record MenuItemAddedDomainEvent(Guid RestaurantId, Guid MenuItemId, string Name) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
