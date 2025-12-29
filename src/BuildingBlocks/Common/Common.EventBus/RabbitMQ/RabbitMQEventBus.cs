using Common.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Common.EventBus.RabbitMQ;

/// <summary>
/// RabbitMQ implementation of event bus
/// </summary>
public class RabbitMQEventBus : IEventBus, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RabbitMQEventBus> _logger;
    private readonly Dictionary<string, List<Type>> _eventHandlers;
    private readonly string _exchangeName;

    public RabbitMQEventBus(
        string hostname,
        IServiceProvider serviceProvider,
        ILogger<RabbitMQEventBus> logger,
        string exchangeName = "food_delivery_events")
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _exchangeName = exchangeName;
        _eventHandlers = new Dictionary<string, List<Type>>();

        var factory = new ConnectionFactory
        {
            HostName = hostname,
            DispatchConsumersAsync = true,
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Declare exchange
        _channel.ExchangeDeclare(
            exchange: _exchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false);

        _logger.LogInformation("RabbitMQ Event Bus initialized");
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent
    {
        var eventName = @event.EventType;
        var message = JsonSerializer.Serialize(@event);
        var body = Encoding.UTF8.GetBytes(message);

        var properties = _channel.CreateBasicProperties();
        properties.DeliveryMode = 2; // Persistent
        properties.ContentType = "application/json";
        properties.MessageId = @event.EventId.ToString();
        properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        _channel.BasicPublish(
            exchange: _exchangeName,
            routingKey: eventName,
            basicProperties: properties,
            body: body);

        _logger.LogInformation("Published event {EventName} with ID {EventId}",
            eventName, @event.EventId);

        await Task.CompletedTask;
    }

    public void Subscribe<TEvent, THandler>()
        where TEvent : IIntegrationEvent
        where THandler : IEventHandler<TEvent>
    {
        var eventName = typeof(TEvent).Name;

        if (!_eventHandlers.ContainsKey(eventName))
        {
            _eventHandlers[eventName] = new List<Type>();

            // Declare queue for this event type
            var queueName = $"{eventName}_queue";
            _channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false);

            // Bind queue to exchange
            _channel.QueueBind(
                queue: queueName,
                exchange: _exchangeName,
                routingKey: eventName);

            // Start consuming
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (sender, args) =>
            {
                await ProcessEvent(eventName, args);
            };

            _channel.BasicConsume(
                queue: queueName,
                autoAck: false,
                consumer: consumer);

            _logger.LogInformation("Subscribed to event {EventName}", eventName);
        }

        _eventHandlers[eventName].Add(typeof(THandler));
    }

    public void Unsubscribe<TEvent, THandler>()
        where TEvent : IIntegrationEvent
        where THandler : IEventHandler<TEvent>
    {
        var eventName = typeof(TEvent).Name;

        if (_eventHandlers.ContainsKey(eventName))
        {
            _eventHandlers[eventName].Remove(typeof(THandler));

            if (_eventHandlers[eventName].Count == 0)
            {
                _eventHandlers.Remove(eventName);
                _logger.LogInformation("Unsubscribed from event {EventName}", eventName);
            }
        }
    }

    private async Task ProcessEvent(string eventName, BasicDeliverEventArgs args)
    {
        try
        {
            var message = Encoding.UTF8.GetString(args.Body.ToArray());

            if (_eventHandlers.ContainsKey(eventName))
            {
                using var scope = _serviceProvider.CreateScope();

                foreach (var handlerType in _eventHandlers[eventName])
                {
                    var handler = scope.ServiceProvider.GetService(handlerType);
                    if (handler == null)
                    {
                        _logger.LogWarning("Handler {HandlerType} not found in DI container", handlerType.Name);
                        continue;
                    }

                    var eventType = handlerType.GetInterfaces()
                        .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventHandler<>))
                        .GetGenericArguments()[0];

                    var @event = JsonSerializer.Deserialize(message, eventType);

                    if (@event != null)
                    {
                        var handleMethod = handlerType.GetMethod(nameof(IEventHandler<IIntegrationEvent>.HandleAsync));
                        if (handleMethod != null)
                        {
                            await (Task)handleMethod.Invoke(handler, new[] { @event, CancellationToken.None })!;
                        }
                    }
                }

                _channel.BasicAck(args.DeliveryTag, false);
                _logger.LogInformation("Event {EventName} processed successfully", eventName);
            }
            else
            {
                _channel.BasicNack(args.DeliveryTag, false, false);
                _logger.LogWarning("No handlers registered for event {EventName}", eventName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing event {EventName}", eventName);
            _channel.BasicNack(args.DeliveryTag, false, true); // Requeue on error
        }
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
