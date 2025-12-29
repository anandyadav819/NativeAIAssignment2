using Common.Contracts.Enums;

namespace Tracking.Application.DTOs;

public class DriverDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string VehicleNumber { get; set; } = string.Empty;
    public DriverStatus Status { get; set; }
    public LocationDto? CurrentLocation { get; set; }
    public Guid? CurrentOrderId { get; set; }
}

public class LocationDto
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime Timestamp { get; set; }
}

public class DeliveryTrackingDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid DriverId { get; set; }
    public LocationDto PickupLocation { get; set; } = null!;
    public LocationDto DeliveryLocation { get; set; } = null!;
    public LocationDto? CurrentLocation { get; set; }
    public DateTime? PickedUpAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public double? EstimatedDistanceKm { get; set; }
    public double? DistanceFromDelivery { get; set; }
}
