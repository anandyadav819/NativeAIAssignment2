using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tracking.Domain.Entities;

namespace Tracking.Infrastructure.Persistence.Configurations;

public class DeliveryTrackingConfiguration : IEntityTypeConfiguration<DeliveryTracking>
{
    public void Configure(EntityTypeBuilder<DeliveryTracking> builder)
    {
        builder.ToTable("delivery_tracking", "tracking");

        builder.HasKey(dt => dt.Id);

        builder.Property(dt => dt.Id)
            .ValueGeneratedNever();

        builder.Property(dt => dt.OrderId)
            .IsRequired();

        builder.Property(dt => dt.DriverId)
            .IsRequired();

        builder.Property(dt => dt.PickedUpAt);

        builder.Property(dt => dt.DeliveredAt);

        builder.Property(dt => dt.EstimatedDistanceKm)
            .HasPrecision(10, 2);

        builder.Property(dt => dt.CreatedAt)
            .IsRequired();

        builder.Property(dt => dt.UpdatedAt)
            .IsRequired();

        // Owned PickupLocation
        builder.OwnsOne(dt => dt.PickupLocation, location =>
        {
            location.Property(l => l.Latitude)
                .HasColumnName("PickupLatitude")
                .IsRequired();

            location.Property(l => l.Longitude)
                .HasColumnName("PickupLongitude")
                .IsRequired();

            location.Property(l => l.Timestamp)
                .HasColumnName("PickupTimestamp")
                .IsRequired();
        });

        // Owned DeliveryLocation
        builder.OwnsOne(dt => dt.DeliveryLocation, location =>
        {
            location.Property(l => l.Latitude)
                .HasColumnName("DeliveryLatitude")
                .IsRequired();

            location.Property(l => l.Longitude)
                .HasColumnName("DeliveryLongitude")
                .IsRequired();

            location.Property(l => l.Timestamp)
                .HasColumnName("DeliveryTimestamp")
                .IsRequired();
        });

        // Owned CurrentLocation
        builder.OwnsOne(dt => dt.CurrentLocation, location =>
        {
            location.Property(l => l.Latitude)
                .HasColumnName("CurrentLatitude");

            location.Property(l => l.Longitude)
                .HasColumnName("CurrentLongitude");

            location.Property(l => l.Timestamp)
                .HasColumnName("CurrentTimestamp");
        });

        // Owned collection for LocationHistory
        builder.OwnsMany(dt => dt.LocationHistory, history =>
        {
            history.ToTable("location_history", "tracking");
            history.WithOwner().HasForeignKey("DeliveryTrackingId");
            
            history.Property<int>("Id").ValueGeneratedOnAdd();
            history.HasKey("Id");

            history.Property(l => l.Latitude)
                .HasColumnName("Latitude")
                .IsRequired();

            history.Property(l => l.Longitude)
                .HasColumnName("Longitude")
                .IsRequired();

            history.Property(l => l.Timestamp)
                .HasColumnName("Timestamp")
                .IsRequired();

            history.HasIndex("DeliveryTrackingId");
        });

        // Indexes
        builder.HasIndex(dt => dt.OrderId).IsUnique();
        builder.HasIndex(dt => dt.DriverId);
        builder.HasIndex(dt => dt.DeliveredAt);
    }
}
