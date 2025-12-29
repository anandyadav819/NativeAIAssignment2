using Common.Contracts.Enums;
using Common.Contracts.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Tracking.Application.Commands;
using Tracking.Domain.Entities;
using Tracking.Domain.Repositories;
using Tracking.Domain.ValueObjects;

namespace Tracking.UnitTests.Application;

public class CreateDeliveryTrackingCommandHandlerTests
{
    private readonly Mock<IDeliveryTrackingRepository> _deliveryTrackingRepositoryMock;
    private readonly Mock<IDriverRepository> _driverRepositoryMock;
    private readonly Mock<ILogger<CreateDeliveryTrackingCommandHandler>> _loggerMock;
    private readonly CreateDeliveryTrackingCommandHandler _handler;

    public CreateDeliveryTrackingCommandHandlerTests()
    {
        _deliveryTrackingRepositoryMock = new Mock<IDeliveryTrackingRepository>();
        _driverRepositoryMock = new Mock<IDriverRepository>();
        _loggerMock = new Mock<ILogger<CreateDeliveryTrackingCommandHandler>>();
        _handler = new CreateDeliveryTrackingCommandHandler(
            _deliveryTrackingRepositoryMock.Object,
            _driverRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidData_CreatesDeliveryTracking()
    {
        // Arrange
        var driver = new Driver("John Doe", "+1234567890", "ABC-123");
        driver.GoOnline();
        var orderId = Guid.NewGuid();
        
        var command = new CreateDeliveryTrackingCommand(
            orderId,
            driver.Id,
            40.7128, -74.0060,
            40.7580, -73.9855);

        _driverRepositoryMock
            .Setup(x => x.GetByIdAsync(driver.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(driver);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        driver.Status.Should().Be(DriverStatus.Busy);
        driver.CurrentOrderId.Should().Be(orderId);
        
        _deliveryTrackingRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<DeliveryTracking>(), It.IsAny<CancellationToken>()), 
            Times.Once);
        
        _driverRepositoryMock.Verify(
            x => x.UpdateAsync(driver, It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithDriverNotFound_ReturnsFailure()
    {
        // Arrange
        var driverId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        
        var command = new CreateDeliveryTrackingCommand(
            orderId,
            driverId,
            40.7128, -74.0060,
            40.7580, -73.9855);

        _driverRepositoryMock
            .Setup(x => x.GetByIdAsync(driverId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Driver?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
        
        _deliveryTrackingRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<DeliveryTracking>(), It.IsAny<CancellationToken>()), 
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithInvalidCoordinates_ReturnsFailure()
    {
        // Arrange
        var driver = new Driver("John Doe", "+1234567890", "ABC-123");
        driver.GoOnline();
        var orderId = Guid.NewGuid();
        
        var command = new CreateDeliveryTrackingCommand(
            orderId,
            driver.Id,
            91, -74.0060,  // Invalid latitude
            40.7580, -73.9855);

        _driverRepositoryMock
            .Setup(x => x.GetByIdAsync(driver.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(driver);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Failed to create delivery tracking");
    }
}
