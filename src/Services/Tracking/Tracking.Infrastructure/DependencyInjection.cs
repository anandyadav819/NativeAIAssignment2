using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tracking.Domain.Repositories;
using Tracking.Infrastructure.Persistence;
using Tracking.Infrastructure.Repositories;

namespace Tracking.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Add DbContext
        services.AddDbContext<TrackingDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("FoodDeliveryDb"),
                npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "tracking");
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorCodesToAdd: null);
                }));

        // Register repositories
        services.AddScoped<IDriverRepository, DriverRepository>();
        services.AddScoped<IDeliveryTrackingRepository, DeliveryTrackingRepository>();

        return services;
    }
}
