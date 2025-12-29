using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;

namespace Common.Resilience;

/// <summary>
/// Extension methods for adding resilience policies to HTTP clients
/// </summary>
public static class ResilienceExtensions
{
    /// <summary>
    /// Adds a resilient HTTP client with retry and circuit breaker policies
    /// </summary>
    public static IHttpClientBuilder AddResilientHttpClient(
        this IServiceCollection services,
        string name,
        Action<HttpClient>? configureClient = null)
    {
        var builder = services.AddHttpClient(name, client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            configureClient?.Invoke(client);
        });

        // Add Polly policies
        builder.AddPolicyHandler((serviceProvider, request) =>
        {
            var logger = serviceProvider.GetService<ILogger<IAsyncPolicy<HttpResponseMessage>>>();
            return ResiliencePolicies.GetCombinedHttpPolicy(logger);
        });

        return builder;
    }

    /// <summary>
    /// Adds a resilient HTTP client with custom policies
    /// </summary>
    public static IHttpClientBuilder AddResilientHttpClient(
        this IServiceCollection services,
        string name,
        IAsyncPolicy<HttpResponseMessage> policy,
        Action<HttpClient>? configureClient = null)
    {
        var builder = services.AddHttpClient(name, client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            configureClient?.Invoke(client);
        });

        builder.AddPolicyHandler(policy);

        return builder;
    }
}
