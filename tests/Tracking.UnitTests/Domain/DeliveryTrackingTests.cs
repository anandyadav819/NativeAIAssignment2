using FluentAssertions;
using Tracking.Domain.Entities;
using Tracking.Domain.ValueObjects;

namespace Tracking.UnitTests.Domain;

public class DeliveryTrackingTests
{
    [Fact]
    public void Constructor_CreatesDeliveryTrackingWithAllProperties()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var driverId = Guid.NewGuid();
        var pickupLocation = new Location(40.7128, -74.0060);
        var deliveryLocation = new Location(40.7580, -73.9855);

        // Act
        var tracking = new DeliveryTracking(orderId, driverId, pickupLocation, deliveryLocation);

        // Assert
        tracking.OrderId.Should().Be(orderId);
        tracking.DriverId.Should().Be(driverId);
        tracking.PickupLocation.Should().Be(pickupLocation);
        tracking.DeliveryLocation.Should().Be(deliveryLocation);
        tracking.CurrentLocation.Should().BeNull();
        tracking.PickedUpAt.Should().BeNull();
        tracking.DeliveredAt.Should().BeNull();
        tracking.EstimatedDistanceKm.Should().BeGreaterThan(0);
        tracking.LocationHistory.Should().BeEmpty();
        tracking.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Constructor_CalculatesEstimatedDistance()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var driverId = Guid.NewGuid();
        var pickupLocation = new Location(40.7128, -74.0060);
        var deliveryLocation = new Location(40.7580, -73.9855);

        // Act
        var tracking = new DeliveryTracking(orderId, driverId, pickupLocation, deliveryLocation);

        // Assert - Distance should be approximately 6km
        tracking.EstimatedDistanceKm.Should().BeApproximately(6, 1);
    }

    [Fact]
    public void UpdateCurrentLocation_UpdatesLocationAndAddsToHistory()
    {
        // Arrange
        var tracking = CreateDeliveryTracking();
        var newLocation = new Location(40.7200, -74.0000);

        // Act
        tracking.UpdateCurrentLocation(newLocation);

        // Assert
        tracking.CurrentLocation.Should().Be(newLocation);
        tracking.LocationHistory.Should().HaveCount(1);
        tracking.LocationHistory[0].Should().Be(newLocation);
        tracking.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void UpdateCurrentLocation_MultipleUpdates_MaintainsHistory()
    {
        // Arrange
        var tracking = CreateDeliveryTracking();
        var location1 = new Location(40.7200, -74.0000);
        var location2 = new Location(40.7300, -73.9950);
        var location3 = new Location(40.7400, -73.9900);

        // Act
        tracking.UpdateCurrentLocation(location1);
        tracking.UpdateCurrentLocation(location2);
        tracking.UpdateCurrentLocation(location3);

        // Assert
        tracking.CurrentLocation.Should().Be(location3);
        tracking.LocationHistory.Should().HaveCount(3);
        tracking.LocationHistory[0].Should().Be(location1);
        tracking.LocationHistory[1].Should().Be(location2);
        tracking.LocationHistory[2].Should().Be(location3);
    }

    [Fact]
    public void MarkPickedUp_SetsPickedUpTime()
    {
        // Arrange
        var tracking = CreateDeliveryTracking();

        // Act
        tracking.MarkPickedUp();

        // Assert
        tracking.PickedUpAt.Should().NotBeNull();
        tracking.PickedUpAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        tracking.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void MarkPickedUp_WhenAlreadyPickedUp_ThrowsInvalidOperationException()
    {
        // Arrange
        var tracking = CreateDeliveryTracking();
        tracking.MarkPickedUp();

        // Act & Assert
        var act = () => tracking.MarkPickedUp();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Order already picked up");
    }

    [Fact]
    public void MarkDelivered_AfterPickup_SetsDeliveredTime()
    {
        // Arrange
        var tracking = CreateDeliveryTracking();
        tracking.MarkPickedUp();

        // Act
        tracking.MarkDelivered();

        // Assert
        tracking.DeliveredAt.Should().NotBeNull();
        tracking.DeliveredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        tracking.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void MarkDelivered_BeforePickup_ThrowsInvalidOperationException()
    {
        // Arrange
        var tracking = CreateDeliveryTracking();

        // Act & Assert
        var act = () => tracking.MarkDelivered();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Order must be picked up before delivery");
    }

    [Fact]
    public void MarkDelivered_WhenAlreadyDelivered_ThrowsInvalidOperationException()
    {
        // Arrange
        var tracking = CreateDeliveryTracking();
        tracking.MarkPickedUp();
        tracking.MarkDelivered();

        // Act & Assert
        var act = () => tracking.MarkDelivered();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Order already delivered");
    }

    [Fact]
    public void GetDistanceFromDelivery_WithCurrentLocation_ReturnsDistance()
    {
        // Arrange
        var tracking = CreateDeliveryTracking();
        var currentLocation = new Location(40.7200, -74.0000);
        tracking.UpdateCurrentLocation(currentLocation);

        // Act
        var distance = tracking.GetDistanceFromDelivery();

        // Assert
        distance.Should().NotBeNull();
        distance.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GetDistanceFromDelivery_WithoutCurrentLocation_ReturnsNull()
    {
        // Arrange
        var tracking = CreateDeliveryTracking();

        // Act
        var distance = tracking.GetDistanceFromDelivery();

        // Assert
        distance.Should().BeNull();
    }

    [Fact]
    public void GetDeliveryDuration_AfterDelivery_ReturnsTimeSpan()
    {
        // Arrange
        var tracking = CreateDeliveryTracking();
        tracking.MarkPickedUp();
        System.Threading.Thread.Sleep(100); // Small delay
        tracking.MarkDelivered();

        // Act
        var duration = tracking.GetDeliveryDuration();

        // Assert
        duration.Should().NotBeNull();
        duration.Should().BeGreaterThan(TimeSpan.Zero);
    }

    [Fact]
    public void GetDeliveryDuration_BeforePickup_ReturnsNull()
    {
        // Arrange
        var tracking = CreateDeliveryTracking();

        // Act
        var duration = tracking.GetDeliveryDuration();

        // Assert
        duration.Should().BeNull();
    }

    [Fact]
    public void GetDeliveryDuration_AfterPickupButBeforeDelivery_ReturnsNull()
    {
        // Arrange
        var tracking = CreateDeliveryTracking();
        tracking.MarkPickedUp();

        // Act
        var duration = tracking.GetDeliveryDuration();

        // Assert
        duration.Should().BeNull();
    }

    private static DeliveryTracking CreateDeliveryTracking()
    {
        var orderId = Guid.NewGuid();
        var driverId = Guid.NewGuid();
        var pickupLocation = new Location(40.7128, -74.0060);
        var deliveryLocation = new Location(40.7580, -73.9855);
        return new DeliveryTracking(orderId, driverId, pickupLocation, deliveryLocation);
    }
}
