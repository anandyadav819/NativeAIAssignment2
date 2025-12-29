using Common.Messaging;
using FluentAssertions;
using Menu.Application.Commands;
using Menu.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using RestaurantEntity = Menu.Domain.Entities.Restaurant;

namespace Menu.UnitTests.Application;

public class CreateRestaurantCommandHandlerTests
{
    private readonly Mock<IRestaurantRepository> _repositoryMock;
    private readonly Mock<ILogger<CreateRestaurantCommandHandler>> _loggerMock;
    private readonly CreateRestaurantCommandHandler _handler;

    public CreateRestaurantCommandHandlerTests()
    {
        _repositoryMock = new Mock<IRestaurantRepository>();
        _loggerMock = new Mock<ILogger<CreateRestaurantCommandHandler>>();
        _handler = new CreateRestaurantCommandHandler(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_Should_CreateRestaurantAndReturnSuccess()
    {
        // Arrange
        var command = new CreateRestaurantCommand(
            "Pizza Palace",
            "Best pizza in town",
            "123 Main St",
            "555-1234",
            new TimeSpan(9, 0, 0),
            new TimeSpan(22, 0, 0),
            5.99m,
            15.00m,
            30);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<RestaurantEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_CreateRestaurantWithCorrectProperties()
    {
        // Arrange
        RestaurantEntity? capturedRestaurant = null;
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<RestaurantEntity>(), It.IsAny<CancellationToken>()))
            .Callback<RestaurantEntity, CancellationToken>((restaurant, _) => capturedRestaurant = restaurant);

        var command = new CreateRestaurantCommand(
            "Pizza Palace",
            "Best pizza",
            "123 Main St",
            "555-1234",
            new TimeSpan(10, 0, 0),
            new TimeSpan(23, 0, 0),
            6.99m,
            20.00m,
            45);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedRestaurant.Should().NotBeNull();
        capturedRestaurant!.Name.Should().Be("Pizza Palace");
        capturedRestaurant.Description.Should().Be("Best pizza");
        capturedRestaurant.Address.Should().Be("123 Main St");
        capturedRestaurant.PhoneNumber.Should().Be("555-1234");
        capturedRestaurant.OpeningTime.Should().Be(new TimeSpan(10, 0, 0));
        capturedRestaurant.ClosingTime.Should().Be(new TimeSpan(23, 0, 0));
        capturedRestaurant.DeliveryFee.Should().Be(6.99m);
        capturedRestaurant.MinimumOrderAmount.Should().Be(20.00m);
        capturedRestaurant.EstimatedPreparationTimeMinutes.Should().Be(45);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_Should_ReturnFailure()
    {
        // Arrange
        var command = new CreateRestaurantCommand(
            "Pizza Palace", "Best pizza", "123 Main St", "555-1234",
            new TimeSpan(9, 0, 0), new TimeSpan(22, 0, 0), 5.99m, 15.00m, 30);

        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<RestaurantEntity>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Failed to create restaurant");
    }
}
