using Common.Caching;
using FluentAssertions;
using Moq;
using Tracking.Application.DTOs;
using Tracking.Application.Queries;
using Tracking.Domain.Entities;
using Tracking.Domain.Repositories;
using Tracking.Domain.ValueObjects;

namespace Tracking.UnitTests.Application;

public class GetDeliveryTrackingByOrderIdQueryHandlerTests
{
    private readonly Mock<IDeliveryTrackingRepository> _deliveryTrackingRepositoryMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly GetDeliveryTrackingByOrderIdQueryHandler _handler;

    public GetDeliveryTrackingByOrderIdQueryHandlerTests()
    {
        _deliveryTrackingRepositoryMock = new Mock<IDeliveryTrackingRepository>();
        _cacheServiceMock = new Mock<ICacheService>();
        _handler = new GetDeliveryTrackingByOrderIdQueryHandler(
            _deliveryTrackingRepositoryMock.Object,
            _cacheServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithExistingTracking_ReturnsTrackingDto()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var driverId = Guid.NewGuid();
        var pickupLocation = new Location(40.7128, -74.0060);
        var deliveryLocation = new Location(40.7580, -73.9855);
        var tracking = new DeliveryTracking(orderId, driverId, pickupLocation, deliveryLocation);

        var query = new GetDeliveryTrackingByOrderIdQuery(orderId);

        _deliveryTrackingRepositoryMock
            .Setup(x => x.GetByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tracking);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.OrderId.Should().Be(orderId);
        result.DriverId.Should().Be(driverId);
        result.PickupLocation.Latitude.Should().Be(40.7128);
        result.DeliveryLocation.Latitude.Should().Be(40.7580);
        result.EstimatedDistanceKm.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Handle_WithCurrentLocation_ReturnsCurrentLocationInDto()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var driverId = Guid.NewGuid();
        var pickupLocation = new Location(40.7128, -74.0060);
        var deliveryLocation = new Location(40.7580, -73.9855);
        var tracking = new DeliveryTracking(orderId, driverId, pickupLocation, deliveryLocation);
        
        var currentLocation = new Location(40.7200, -74.0000);
        tracking.UpdateCurrentLocation(currentLocation);

        var query = new GetDeliveryTrackingByOrderIdQuery(orderId);

        _deliveryTrackingRepositoryMock
            .Setup(x => x.GetByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tracking);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.CurrentLocation.Should().NotBeNull();
        result.CurrentLocation!.Latitude.Should().Be(40.7200);
        result.DistanceFromDelivery.Should().NotBeNull();
        result.DistanceFromDelivery.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Handle_WithNonExistingTracking_ReturnsNull()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var query = new GetDeliveryTrackingByOrderIdQuery(orderId);

        _deliveryTrackingRepositoryMock
            .Setup(x => x.GetByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DeliveryTracking?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}
