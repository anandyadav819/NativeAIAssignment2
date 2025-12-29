using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Order.Domain.Entities;

namespace Order.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Domain.Entities.Order>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Order> builder)
    {
        builder.ToTable("orders", "order");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .ValueGeneratedNever();

        builder.Property(o => o.CustomerId)
            .IsRequired();

        builder.Property(o => o.RestaurantId)
            .IsRequired();

        builder.Property(o => o.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(o => o.TotalAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(o => o.DeliveryAddress)
            .HasMaxLength(500);

        builder.Property(o => o.DriverId);

        builder.Property(o => o.Version)
            .IsConcurrencyToken()
            .IsRequired();

        builder.Property(o => o.CreatedAt)
            .IsRequired();

        builder.Property(o => o.UpdatedAt)
            .IsRequired();

        // Owned collection for order items
        builder.OwnsMany(o => o.Items, items =>
        {
            items.ToTable("order_items", "order");
            
            items.WithOwner().HasForeignKey("OrderId");
            
            items.HasKey("Id");
            
            items.Property(i => i.Id)
                .ValueGeneratedNever()
                .HasColumnName("Id");

            items.Property(i => i.MenuItemId)
                .IsRequired()
                .HasColumnName("MenuItemId");

            items.Property(i => i.Name)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnName("Name");

            items.Property(i => i.Quantity)
                .IsRequired()
                .HasColumnName("Quantity");

            items.Property(i => i.UnitPrice)
                .HasPrecision(18, 2)
                .IsRequired()
                .HasColumnName("UnitPrice");
        });

        // Indexes
        builder.HasIndex(o => o.CustomerId);
        builder.HasIndex(o => o.RestaurantId);
        builder.HasIndex(o => o.DriverId);
        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => o.CreatedAt);
    }
}
