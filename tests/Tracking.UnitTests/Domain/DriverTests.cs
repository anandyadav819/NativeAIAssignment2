using Common.Contracts.Enums;
using FluentAssertions;
using Tracking.Domain.Entities;
using Tracking.Domain.Events;

namespace Tracking.UnitTests.Domain;

public class DriverTests
{
    [Fact]
    public void Constructor_CreatesDriverWithOfflineStatus()
    {
        // Arrange & Act
        var driver = new Driver("John Doe", "+1234567890", "ABC-123");

        // Assert
        driver.Name.Should().Be("John Doe");
        driver.PhoneNumber.Should().Be("+1234567890");
        driver.VehicleNumber.Should().Be("ABC-123");
        driver.Status.Should().Be(DriverStatus.Offline);
        driver.CurrentOrderId.Should().BeNull();
        driver.CurrentLocation.Should().BeNull();
        driver.Id.Should().NotBeEmpty();
        driver.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        driver.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Constructor_RaiseDriverCreatedEvent()
    {
        // Arrange & Act
        var driver = new Driver("John Doe", "+1234567890", "ABC-123");

        // Assert
        var domainEvent = driver.DomainEvents.OfType<DriverCreatedDomainEvent>().FirstOrDefault();
        domainEvent.Should().NotBeNull();
        domainEvent!.DriverId.Should().Be(driver.Id);
        domainEvent.Name.Should().Be("John Doe");
    }

    [Fact]
    public void GoOnline_FromOfflineStatus_ChangesStatusToAvailable()
    {
        // Arrange
        var driver = new Driver("John Doe", "+1234567890", "ABC-123");
        driver.ClearDomainEvents();

        // Act
        driver.GoOnline();

        // Assert
        driver.Status.Should().Be(DriverStatus.Available);
        driver.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        
        var statusEvent = driver.DomainEvents.OfType<DriverStatusChangedDomainEvent>().FirstOrDefault();
        statusEvent.Should().NotBeNull();
        statusEvent!.NewStatus.Should().Be(DriverStatus.Available);
    }

    [Fact]
    public void GoOnline_WhenAlreadyOnline_ThrowsInvalidOperationException()
    {
        // Arrange
        var driver = new Driver("John Doe", "+1234567890", "ABC-123");
        driver.GoOnline();

        // Act & Assert
        var act = () => driver.GoOnline();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Driver is already online");
    }

    [Fact]
    public void GoOffline_WhenAvailable_ChangesStatusToOffline()
    {
        // Arrange
        var driver = new Driver("John Doe", "+1234567890", "ABC-123");
        driver.GoOnline();
        driver.ClearDomainEvents();

        // Act
        driver.GoOffline();

        // Assert
        driver.Status.Should().Be(DriverStatus.Offline);
        
        var statusEvent = driver.DomainEvents.OfType<DriverStatusChangedDomainEvent>().FirstOrDefault();
        statusEvent.Should().NotBeNull();
        statusEvent!.NewStatus.Should().Be(DriverStatus.Offline);
    }

    [Fact]
    public void GoOffline_WithActiveOrder_ThrowsInvalidOperationException()
    {
        // Arrange
        var driver = new Driver("John Doe", "+1234567890", "ABC-123");
        driver.GoOnline();
        var orderId = Guid.NewGuid();
        driver.AssignOrder(orderId);

        // Act & Assert
        var act = () => driver.GoOffline();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot go offline while on active delivery");
    }

    [Fact]
    public void AssignOrder_WhenAvailable_AssignsOrderAndChangeStatusToBusy()
    {
        // Arrange
        var driver = new Driver("John Doe", "+1234567890", "ABC-123");
        driver.GoOnline();
        driver.ClearDomainEvents();
        var orderId = Guid.NewGuid();

        // Act
        driver.AssignOrder(orderId);

        // Assert
        driver.CurrentOrderId.Should().Be(orderId);
        driver.Status.Should().Be(DriverStatus.Busy);
        
        var assignEvent = driver.DomainEvents.OfType<DriverAssignedToOrderDomainEvent>().FirstOrDefault();
        assignEvent.Should().NotBeNull();
        assignEvent!.DriverId.Should().Be(driver.Id);
        assignEvent.OrderId.Should().Be(orderId);
    }

    [Fact]
    public void AssignOrder_WhenNotAvailable_ThrowsInvalidOperationException()
    {
        // Arrange
        var driver = new Driver("John Doe", "+1234567890", "ABC-123");
        var orderId = Guid.NewGuid();

        // Act & Assert
        var act = () => driver.AssignOrder(orderId);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Driver must be online to accept orders");
    }

    [Fact]
    public void AssignOrder_WithExistingOrder_ThrowsInvalidOperationException()
    {
        // Arrange
        var driver = new Driver("John Doe", "+1234567890", "ABC-123");
        driver.GoOnline();
        driver.AssignOrder(Guid.NewGuid());

        // Act & Assert
        var act = () => driver.AssignOrder(Guid.NewGuid());
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Driver must be online to accept orders");
    }

    [Fact]
    public void CompleteDelivery_WithActiveOrder_CompletesOrderAndBecomesAvailable()
    {
        // Arrange
        var driver = new Driver("John Doe", "+1234567890", "ABC-123");
        driver.GoOnline();
        var orderId = Guid.NewGuid();
        driver.AssignOrder(orderId);
        driver.ClearDomainEvents();

        // Act
        driver.CompleteDelivery();

        // Assert
        driver.CurrentOrderId.Should().BeNull();
        driver.Status.Should().Be(DriverStatus.Available);
        
        var completedEvent = driver.DomainEvents.OfType<DeliveryCompletedDomainEvent>().FirstOrDefault();
        completedEvent.Should().NotBeNull();
        completedEvent!.DriverId.Should().Be(driver.Id);
        completedEvent.OrderId.Should().Be(orderId);
    }

    [Fact]
    public void CompleteDelivery_WithoutActiveOrder_ThrowsInvalidOperationException()
    {
        // Arrange
        var driver = new Driver("John Doe", "+1234567890", "ABC-123");
        driver.GoOnline();

        // Act & Assert
        var act = () => driver.CompleteDelivery();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("No active order to complete");
    }

    [Fact]
    public void UpdateLocation_UpdatesCurrentLocation()
    {
        // Arrange
        var driver = new Driver("John Doe", "+1234567890", "ABC-123");
        driver.ClearDomainEvents();

        // Act
        driver.UpdateLocation(40.7128, -74.0060);

        // Assert
        driver.CurrentLocation.Should().NotBeNull();
        driver.CurrentLocation!.Latitude.Should().Be(40.7128);
        driver.CurrentLocation.Longitude.Should().Be(-74.0060);
        driver.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void UpdateLocation_RaisesDriverLocationUpdatedEvent()
    {
        // Arrange
        var driver = new Driver("John Doe", "+1234567890", "ABC-123");
        driver.GoOnline();
        var orderId = Guid.NewGuid();
        driver.AssignOrder(orderId);
        driver.ClearDomainEvents();

        // Act
        driver.UpdateLocation(40.7128, -74.0060);

        // Assert
        var locationEvent = driver.DomainEvents.OfType<DriverLocationUpdatedDomainEvent>().FirstOrDefault();
        locationEvent.Should().NotBeNull();
        locationEvent!.DriverId.Should().Be(driver.Id);
        locationEvent.Latitude.Should().Be(40.7128);
        locationEvent.Longitude.Should().Be(-74.0060);
        locationEvent.OrderId.Should().Be(orderId);
    }

    [Fact]
    public void UpdateLocation_WithoutActiveOrder_EventHasNullOrderId()
    {
        // Arrange
        var driver = new Driver("John Doe", "+1234567890", "ABC-123");
        driver.ClearDomainEvents();

        // Act
        driver.UpdateLocation(40.7128, -74.0060);

        // Assert
        var locationEvent = driver.DomainEvents.OfType<DriverLocationUpdatedDomainEvent>().FirstOrDefault();
        locationEvent.Should().NotBeNull();
        locationEvent!.OrderId.Should().BeNull();
    }
}
