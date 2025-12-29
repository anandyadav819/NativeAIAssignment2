using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tracking.Domain.Entities;

namespace Tracking.Infrastructure.Persistence.Configurations;

public class DriverConfiguration : IEntityTypeConfiguration<Driver>
{
    public void Configure(EntityTypeBuilder<Driver> builder)
    {
        builder.ToTable("drivers", "tracking");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id)
            .ValueGeneratedNever();

        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(d => d.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(d => d.VehicleNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(d => d.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(d => d.CurrentOrderId);

        builder.Property(d => d.CreatedAt)
            .IsRequired();

        builder.Property(d => d.UpdatedAt)
            .IsRequired();

        // Owned CurrentLocation
        builder.OwnsOne(d => d.CurrentLocation, location =>
        {
            location.Property(l => l.Latitude)
                .HasColumnName("CurrentLatitude");

            location.Property(l => l.Longitude)
                .HasColumnName("CurrentLongitude");

            location.Property(l => l.Timestamp)
                .HasColumnName("LocationTimestamp");
        });

        // Indexes
        builder.HasIndex(d => d.Status);
        builder.HasIndex(d => d.CurrentOrderId);
    }
}
