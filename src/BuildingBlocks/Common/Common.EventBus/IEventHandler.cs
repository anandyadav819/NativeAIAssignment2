using Common.Messaging;

namespace Common.EventBus;

/// <summary>
/// Interface for event handlers
/// </summary>
public interface IEventHandler<in TEvent> where TEvent : IIntegrationEvent
{
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}
