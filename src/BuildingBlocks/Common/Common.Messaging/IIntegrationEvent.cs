namespace Common.Messaging;

/// <summary>
/// Interface for integration events that are published between services
/// </summary>
public interface IIntegrationEvent : IEvent
{
    string EventType { get; }
}
