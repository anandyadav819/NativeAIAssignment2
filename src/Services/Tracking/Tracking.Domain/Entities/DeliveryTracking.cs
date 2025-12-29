using Common.Domain;
using Tracking.Domain.ValueObjects;

namespace Tracking.Domain.Entities;

/// <summary>
/// Delivery tracking aggregate for order tracking
/// </summary>
public class DeliveryTracking : AggregateRoot<Guid>
{
    private readonly List<Location> _locationHistory = new();

    public Guid OrderId { get; private set; }
    public Guid DriverId { get; private set; }
    public Location PickupLocation { get; private set; }
    public Location DeliveryLocation { get; private set; }
    public Location? CurrentLocation { get; private set; }
    public DateTime? PickedUpAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public double? EstimatedDistanceKm { get; private set; }
    public IReadOnlyList<Location> LocationHistory => _locationHistory.AsReadOnly();

    private DeliveryTracking()
    {
        PickupLocation = null!;
        DeliveryLocation = null!;
    } // EF Core

    public DeliveryTracking(
        Guid orderId,
        Guid driverId,
        Location pickupLocation,
        Location deliveryLocation)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        DriverId = driverId;
        PickupLocation = pickupLocation;
        DeliveryLocation = deliveryLocation;
        EstimatedDistanceKm = pickupLocation.DistanceTo(deliveryLocation);
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateCurrentLocation(Location location)
    {
        CurrentLocation = location;
        _locationHistory.Add(location);
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkPickedUp()
    {
        if (PickedUpAt.HasValue)
            throw new InvalidOperationException("Order already picked up");

        PickedUpAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkDelivered()
    {
        if (!PickedUpAt.HasValue)
            throw new InvalidOperationException("Order must be picked up before delivery");

        if (DeliveredAt.HasValue)
            throw new InvalidOperationException("Order already delivered");

        DeliveredAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public double? GetDistanceFromDelivery()
    {
        if (CurrentLocation == null)
            return null;

        return CurrentLocation.DistanceTo(DeliveryLocation);
    }

    public TimeSpan? GetDeliveryDuration()
    {
        if (!PickedUpAt.HasValue || !DeliveredAt.HasValue)
            return null;

        return DeliveredAt.Value - PickedUpAt.Value;
    }
}
