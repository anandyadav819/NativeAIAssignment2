using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;

namespace Common.Resilience;

/// <summary>
/// Pre-configured Polly policies for resilience
/// </summary>
public static class ResiliencePolicies
{
    /// <summary>
    /// HTTP retry policy with exponential backoff
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetHttpRetryPolicy(ILogger? logger = null)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    logger?.LogWarning(
                        "Retry {RetryCount} after {Delay}s due to: {Reason}",
                        retryCount,
                        timespan.TotalSeconds,
                        outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString() ?? "Unknown");
                });
    }

    /// <summary>
    /// HTTP circuit breaker policy
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(ILogger? logger = null)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (outcome, duration) =>
                {
                    logger?.LogWarning(
                        "Circuit breaker opened for {Duration}s due to: {Reason}",
                        duration.TotalSeconds,
                        outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString() ?? "Unknown");
                },
                onReset: () =>
                {
                    logger?.LogInformation("Circuit breaker reset");
                });
    }

    /// <summary>
    /// HTTP timeout policy
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy(TimeSpan timeout)
    {
        return Policy.TimeoutAsync<HttpResponseMessage>(timeout);
    }

    /// <summary>
    /// Combined HTTP policy (timeout + retry + circuit breaker)
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetCombinedHttpPolicy(ILogger? logger = null)
    {
        var timeout = GetTimeoutPolicy(TimeSpan.FromSeconds(10));
        var retry = GetHttpRetryPolicy(logger);
        var circuitBreaker = GetCircuitBreakerPolicy(logger);

        return Policy.WrapAsync(timeout, retry, circuitBreaker);
    }

    /// <summary>
    /// Generic retry policy for database operations
    /// </summary>
    public static IAsyncPolicy GetDatabaseRetryPolicy(ILogger? logger = null)
    {
        return Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (exception, timespan, retryCount, context) =>
                {
                    logger?.LogWarning(
                        exception,
                        "Database retry {RetryCount} after {Delay}s",
                        retryCount,
                        timespan.TotalSeconds);
                });
    }

    /// <summary>
    /// Generic retry policy for cache operations
    /// </summary>
    public static IAsyncPolicy GetCacheRetryPolicy(ILogger? logger = null)
    {
        return Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 2,
                sleepDurationProvider: retryAttempt => TimeSpan.FromMilliseconds(100 * retryAttempt),
                onRetry: (exception, timespan, retryCount, context) =>
                {
                    logger?.LogWarning(
                        exception,
                        "Cache retry {RetryCount} after {Delay}ms",
                        retryCount,
                        timespan.TotalMilliseconds);
                });
    }
}
