using Common.EventBus.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Common.EventBus;

/// <summary>
/// Extension methods for registering event bus services
/// </summary>
public static class EventBusExtensions
{
    public static IServiceCollection AddRabbitMQEventBus(
        this IServiceCollection services,
        string hostname,
        string exchangeName = "food_delivery_events")
    {
        services.AddSingleton<IEventBus>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<RabbitMQEventBus>>();
            return new RabbitMQEventBus(hostname, sp, logger, exchangeName);
        });

        return services;
    }
}
