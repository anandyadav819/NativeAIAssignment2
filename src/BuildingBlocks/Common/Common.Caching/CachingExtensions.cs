using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Common.Caching;

/// <summary>
/// Extension methods for registering caching services
/// </summary>
public static class CachingExtensions
{
    public static IServiceCollection AddHybridCache(
        this IServiceCollection services,
        string redisConnectionString)
    {
        // Add Memory Cache (L1)
        //services.AddMemoryCache(options =>
        //{
        //    options.SizeLimit = 1024; // Limit memory cache size
        //});

        services.AddMemoryCache(options =>
        {
            //options.SizeLimit = 1024; // Limit memory cache size
        });

        // Add Redis (L2)
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var configuration = ConfigurationOptions.Parse(redisConnectionString);
            configuration.AbortOnConnectFail = false;
            configuration.ConnectTimeout = 5000;
            configuration.SyncTimeout = 5000;
            
            return ConnectionMultiplexer.Connect(configuration);
        });

        // Add Hybrid Cache Service
        services.AddSingleton<ICacheService, HybridCacheService>();

        return services;
    }
}
