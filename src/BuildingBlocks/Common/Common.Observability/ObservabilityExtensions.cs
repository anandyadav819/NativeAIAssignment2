using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;

namespace Common.Observability;

/// <summary>
/// Extension methods for configuring observability (logging, tracing, metrics)
/// </summary>
public static class ObservabilityExtensions
{
    public static WebApplicationBuilder AddObservability(
        this WebApplicationBuilder builder,
        string serviceName)
    {
        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("ServiceName", serviceName)
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{ServiceName}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(Log.Logger);

        // Configure OpenTelemetry Tracing
        builder.Services.AddOpenTelemetry()
            .WithTracing(tracerProviderBuilder =>
            {
                tracerProviderBuilder
                    .SetResourceBuilder(ResourceBuilder.CreateDefault()
                        .AddService(serviceName))
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                    })
                    .AddHttpClientInstrumentation()
                    .AddSource(serviceName);
            });

        return builder;
    }

    public static IServiceCollection AddHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var healthChecksBuilder = services.AddHealthChecks();

        // Add PostgreSQL health check if connection string exists
        var dbConnection = configuration.GetConnectionString("Database");
        if (!string.IsNullOrEmpty(dbConnection))
        {
            healthChecksBuilder.AddNpgSql(
                dbConnection,
                name: "database",
                timeout: TimeSpan.FromSeconds(3),
                tags: new[] { "ready" });
        }

        // Add Redis health check if connection string exists
        var redisConnection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnection))
        {
            healthChecksBuilder.AddRedis(
                redisConnection,
                name: "redis",
                timeout: TimeSpan.FromSeconds(3),
                tags: new[] { "ready" });
        }

        return services;
    }

    public static WebApplication MapObservabilityEndpoints(this WebApplication app)
    {
        // Health check endpoints
        app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = _ => false // Liveness check (no dependencies)
        });

        app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready") // Readiness check (dependencies)
        });

        return app;
    }
}
