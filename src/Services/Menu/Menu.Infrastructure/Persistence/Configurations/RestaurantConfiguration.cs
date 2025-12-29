using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Menu.Domain.Entities;

namespace Menu.Infrastructure.Persistence.Configurations;

public class RestaurantConfiguration : IEntityTypeConfiguration<Restaurant>
{
    public void Configure(EntityTypeBuilder<Restaurant> builder)
    {
        builder.ToTable("restaurants", "menu");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .ValueGeneratedNever();

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(r => r.Address)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(r => r.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(r => r.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(r => r.DeliveryFee)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(r => r.MinimumOrderAmount)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(r => r.EstimatedPreparationTimeMinutes)
            .IsRequired();

        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.Property(r => r.UpdatedAt)
            .IsRequired();

        // Owned collection for menu items
        builder.OwnsMany(r => r.MenuItems, items =>
        {
            items.ToTable("menu_items", "menu");
            
            items.WithOwner().HasForeignKey("RestaurantId");
            
            items.HasKey("Id");
            
            items.Property(i => i.Id)
                .ValueGeneratedNever()
                .HasColumnName("Id");

            items.Property(i => i.RestaurantId)
                .IsRequired()
                .HasColumnName("RestaurantId");

            items.Property(i => i.Name)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnName("Name");

            items.Property(i => i.Description)
                .IsRequired()
                .HasMaxLength(1000)
                .HasColumnName("Description");

            items.Property(i => i.Price)
                .HasPrecision(10, 2)
                .IsRequired()
                .HasColumnName("Price");

            items.Property(i => i.Category)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("Category");

            items.Property(i => i.ImageUrl)
                .HasMaxLength(500)
                .HasColumnName("ImageUrl");

            items.Property(i => i.IsAvailable)
                .IsRequired()
                .HasColumnName("IsAvailable");

            items.Property(i => i.CreatedAt)
                .IsRequired()
                .HasColumnName("CreatedAt");

            items.Property(i => i.UpdatedAt)
                .IsRequired()
                .HasColumnName("UpdatedAt");

            // Indexes on menu_items table
            items.HasIndex("RestaurantId");
            items.HasIndex("Category");
            items.HasIndex("IsAvailable");
        });

        // Indexes on restaurants table
        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.Name);
    }
}
