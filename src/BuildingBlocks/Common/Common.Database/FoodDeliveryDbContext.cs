using Microsoft.EntityFrameworkCore;
using Common.Domain;

namespace Common.Database;

/// <summary>
/// Base DbContext for the Food Delivery platform
/// All service entities are registered here with schema separation
/// </summary>
public class FoodDeliveryDbContext : DbContext
{
    public FoodDeliveryDbContext(DbContextOptions<FoodDeliveryDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FoodDeliveryDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Update timestamps before saving
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity is Entity<Guid> entity)
            {
                entity.GetType()
                    .GetProperty(nameof(Entity<Guid>.UpdatedAt))
                    ?.SetValue(entity, DateTime.UtcNow);
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
