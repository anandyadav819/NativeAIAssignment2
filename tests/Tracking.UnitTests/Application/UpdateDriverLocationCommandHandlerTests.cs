using Common.Contracts.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Tracking.Application.Commands;
using Tracking.Domain.Entities;
using Tracking.Domain.Repositories;

namespace Tracking.UnitTests.Application;

public class UpdateDriverLocationCommandHandlerTests
{
    private readonly Mock<IDriverRepository> _driverRepositoryMock;
    private readonly Mock<IDeliveryTrackingRepository> _deliveryTrackingRepositoryMock;
    private readonly Mock<ILogger<UpdateDriverLocationCommandHandler>> _loggerMock;
    private readonly UpdateDriverLocationCommandHandler _handler;

    public UpdateDriverLocationCommandHandlerTests()
    {
        _driverRepositoryMock = new Mock<IDriverRepository>();
        _deliveryTrackingRepositoryMock = new Mock<IDeliveryTrackingRepository>();
        _loggerMock = new Mock<ILogger<UpdateDriverLocationCommandHandler>>();
        _handler = new UpdateDriverLocationCommandHandler(
            _driverRepositoryMock.Object,
            _deliveryTrackingRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidDriver_UpdatesDriverLocation()
    {
        // Arrange
        var driver = new Driver("John Doe", "+1234567890", "ABC-123");
        var command = new UpdateDriverLocationCommand(driver.Id, 40.7128, -74.0060);

        _driverRepositoryMock
            .Setup(x => x.GetByIdAsync(driver.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(driver);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        driver.CurrentLocation.Should().NotBeNull();
        driver.CurrentLocation!.Latitude.Should().Be(40.7128);
        driver.CurrentLocation.Longitude.Should().Be(-74.0060);
        
        _driverRepositoryMock.Verify(
            x => x.UpdateAsync(driver, It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithDriverNotFound_ReturnsFailure()
    {
        // Arrange
        var driverId = Guid.NewGuid();
        var command = new UpdateDriverLocationCommand(driverId, 40.7128, -74.0060);

        _driverRepositoryMock
            .Setup(x => x.GetByIdAsync(driverId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Driver?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
        
        _driverRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Driver>(), It.IsAny<CancellationToken>()), 
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithActiveOrder_UpdatesDeliveryTracking()
    {
        // Arrange
        var driver = new Driver("John Doe", "+1234567890", "ABC-123");
        driver.GoOnline();
        var orderId = Guid.NewGuid();
        driver.AssignOrder(orderId);

        var pickupLocation = new Tracking.Domain.ValueObjects.Location(40.7128, -74.0060);
        var deliveryLocation = new Tracking.Domain.ValueObjects.Location(40.7580, -73.9855);
        var tracking = new DeliveryTracking(orderId, driver.Id, pickupLocation, deliveryLocation);

        var command = new UpdateDriverLocationCommand(driver.Id, 40.7200, -74.0000);

        _driverRepositoryMock
            .Setup(x => x.GetByIdAsync(driver.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(driver);

        _deliveryTrackingRepositoryMock
            .Setup(x => x.GetByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tracking);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        tracking.CurrentLocation.Should().NotBeNull();
        tracking.CurrentLocation!.Latitude.Should().Be(40.7200);
        tracking.CurrentLocation.Longitude.Should().Be(-74.0000);
        
        _deliveryTrackingRepositoryMock.Verify(
            x => x.UpdateAsync(tracking, It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithoutActiveOrder_DoesNotUpdateTracking()
    {
        // Arrange
        var driver = new Driver("John Doe", "+1234567890", "ABC-123");
        driver.GoOnline();
        var command = new UpdateDriverLocationCommand(driver.Id, 40.7128, -74.0060);

        _driverRepositoryMock
            .Setup(x => x.GetByIdAsync(driver.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(driver);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        _deliveryTrackingRepositoryMock.Verify(
            x => x.GetByOrderIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), 
            Times.Never);
        
        _deliveryTrackingRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<DeliveryTracking>(), It.IsAny<CancellationToken>()), 
            Times.Never);
    }
}
