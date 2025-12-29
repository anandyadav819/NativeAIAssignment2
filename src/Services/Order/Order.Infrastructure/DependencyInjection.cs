using Common.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Order.Application.Services;
using Order.Domain.Repositories;
using Order.Infrastructure.Persistence;
using Order.Infrastructure.Repositories;
using Order.Infrastructure.Services;

namespace Order.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Add DbContext
        services.AddDbContext<OrderDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("FoodDeliveryDb"),
                npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "order");
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorCodesToAdd: null);
                }));

        // Register repositories
        services.AddScoped<IOrderRepository, OrderRepository>();

        // Register payment gateway
        services.AddScoped<IPaymentGateway, MockPaymentGateway>();

        return services;
    }
}
