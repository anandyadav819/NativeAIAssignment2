namespace Common.Messaging;

/// <summary>
/// Marker interface for all events
/// </summary>
public interface IEvent
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
}
