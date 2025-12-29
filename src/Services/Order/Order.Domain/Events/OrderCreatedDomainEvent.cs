using Common.Domain;

namespace Order.Domain.Events;

public record OrderCreatedDomainEvent(
    Guid OrderId,
    Guid CustomerId,
    Guid RestaurantId) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
