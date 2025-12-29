using Common.Contracts.Enums;
using FluentAssertions;
using Moq;
using Tracking.Application.DTOs;
using Tracking.Application.Queries;
using Tracking.Domain.Entities;
using Tracking.Domain.Repositories;
using Tracking.Domain.ValueObjects;

namespace Tracking.UnitTests.Application;

public class GetAvailableDriversQueryHandlerTests
{
    private readonly Mock<IDriverRepository> _driverRepositoryMock;
    private readonly GetAvailableDriversQueryHandler _handler;

    public GetAvailableDriversQueryHandlerTests()
    {
        _driverRepositoryMock = new Mock<IDriverRepository>();
        _handler = new GetAvailableDriversQueryHandler(_driverRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithAvailableDrivers_ReturnsDriverDtos()
    {
        // Arrange
        var driver1 = new Driver("John Doe", "+1234567890", "ABC-123");
        driver1.GoOnline();
        driver1.UpdateLocation(40.7128, -74.0060);

        var driver2 = new Driver("Jane Smith", "+0987654321", "XYZ-789");
        driver2.GoOnline();

        var drivers = new List<Driver> { driver1, driver2 };
        var query = new GetAvailableDriversQuery();

        _driverRepositoryMock
            .Setup(x => x.GetAvailableDriversAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(drivers);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        
        var dto1 = result.First(d => d.Name == "John Doe");
        dto1.PhoneNumber.Should().Be("+1234567890");
        dto1.VehicleNumber.Should().Be("ABC-123");
        dto1.Status.Should().Be(DriverStatus.Available);
        dto1.CurrentLocation.Should().NotBeNull();
        dto1.CurrentLocation!.Latitude.Should().Be(40.7128);

        var dto2 = result.First(d => d.Name == "Jane Smith");
        dto2.CurrentLocation.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithNoAvailableDrivers_ReturnsEmptyList()
    {
        // Arrange
        var query = new GetAvailableDriversQuery();

        _driverRepositoryMock
            .Setup(x => x.GetAvailableDriversAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Driver>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithDriversWithCurrentOrder_IncludesOrderId()
    {
        // Arrange
        var driver = new Driver("John Doe", "+1234567890", "ABC-123");
        driver.GoOnline();
        var orderId = Guid.NewGuid();
        driver.AssignOrder(orderId);

        var drivers = new List<Driver> { driver };
        var query = new GetAvailableDriversQuery();

        _driverRepositoryMock
            .Setup(x => x.GetAvailableDriversAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(drivers);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].CurrentOrderId.Should().Be(orderId);
        result[0].Status.Should().Be(DriverStatus.Busy);
    }
}
