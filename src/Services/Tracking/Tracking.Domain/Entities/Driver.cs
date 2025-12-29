using Common.Contracts.Enums;
using Common.Domain;
using Tracking.Domain.Events;
using Tracking.Domain.ValueObjects;

namespace Tracking.Domain.Entities;

/// <summary>
/// Driver aggregate root
/// </summary>
public class Driver : AggregateRoot<Guid>
{
    public string Name { get; private set; }
    public string PhoneNumber { get; private set; }
    public string VehicleNumber { get; private set; }
    public DriverStatus Status { get; private set; }
    public Location? CurrentLocation { get; private set; }
    public Guid? CurrentOrderId { get; private set; }

    private Driver()
    {
        Name = string.Empty;
        PhoneNumber = string.Empty;
        VehicleNumber = string.Empty;
    } // EF Core

    public Driver(string name, string phoneNumber, string vehicleNumber)
    {
        Id = Guid.NewGuid();
        Name = name;
        PhoneNumber = phoneNumber;
        VehicleNumber = vehicleNumber;
        Status = DriverStatus.Offline;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new DriverCreatedDomainEvent(Id, Name));
    }

    public void GoOnline()
    {
        if (Status == DriverStatus.Available)
            throw new InvalidOperationException("Driver is already online");

        Status = DriverStatus.Available;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new DriverStatusChangedDomainEvent(Id, Status));
    }

    public void GoOffline()
    {
        if (CurrentOrderId.HasValue)
            throw new InvalidOperationException("Cannot go offline while on active delivery");

        Status = DriverStatus.Offline;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new DriverStatusChangedDomainEvent(Id, Status));
    }

    public void AssignOrder(Guid orderId)
    {
        if (Status != DriverStatus.Available)
            throw new InvalidOperationException("Driver must be online to accept orders");

        if (CurrentOrderId.HasValue)
            throw new InvalidOperationException("Driver already has an active order");

        CurrentOrderId = orderId;
        Status = DriverStatus.Busy;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new DriverAssignedToOrderDomainEvent(Id, orderId));
    }

    public void CompleteDelivery()
    {
        if (!CurrentOrderId.HasValue)
            throw new InvalidOperationException("No active order to complete");

        var orderId = CurrentOrderId.Value;
        CurrentOrderId = null;
        Status = DriverStatus.Available;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new DeliveryCompletedDomainEvent(Id, orderId));
    }

    public void UpdateLocation(double latitude, double longitude)
    {
        var newLocation = new Location(latitude, longitude);
        CurrentLocation = newLocation;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new DriverLocationUpdatedDomainEvent(
            Id, 
            latitude, 
            longitude, 
            CurrentOrderId));
    }
}
