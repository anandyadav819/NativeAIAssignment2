using FluentAssertions;
using Menu.Application.Commands;
using Menu.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using RestaurantEntity = Menu.Domain.Entities.Restaurant;

namespace Menu.UnitTests.Application;

public class AddMenuItemCommandHandlerTests
{
    private readonly Mock<IRestaurantRepository> _repositoryMock;
    private readonly Mock<ILogger<AddMenuItemCommandHandler>> _loggerMock;
    private readonly AddMenuItemCommandHandler _handler;

    public AddMenuItemCommandHandlerTests()
    {
        _repositoryMock = new Mock<IRestaurantRepository>();
        _loggerMock = new Mock<ILogger<AddMenuItemCommandHandler>>();
        _handler = new AddMenuItemCommandHandler(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_Should_AddMenuItemAndReturnSuccess()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();
        var restaurant = CreateTestRestaurant();

        _repositoryMock.Setup(r => r.GetByIdAsync(restaurantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(restaurant);

        var command = new AddMenuItemCommand(
            restaurantId, "Margherita Pizza", "Classic pizza", 12.99m, "Pizza", "pizza.jpg");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        restaurant.MenuItems.Should().HaveCount(1);
        restaurant.MenuItems.First().Name.Should().Be("Margherita Pizza");
        _repositoryMock.Verify(r => r.UpdateAsync(restaurant, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRestaurantNotFound_Should_ReturnFailure()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetByIdAsync(restaurantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RestaurantEntity?)null);

        var command = new AddMenuItemCommand(
            restaurantId, "Margherita Pizza", "Classic pizza", 12.99m, "Pizza");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<RestaurantEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_AddMenuItemWithCorrectProperties()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();
        var restaurant = CreateTestRestaurant();

        _repositoryMock.Setup(r => r.GetByIdAsync(restaurantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(restaurant);

        var command = new AddMenuItemCommand(
            restaurantId, "Pepperoni Pizza", "Spicy pizza with pepperoni", 14.99m, "Pizza", "pepperoni.jpg");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var menuItem = restaurant.MenuItems.First();
        menuItem.Name.Should().Be("Pepperoni Pizza");
        menuItem.Description.Should().Be("Spicy pizza with pepperoni");
        menuItem.Price.Should().Be(14.99m);
        menuItem.Category.Should().Be("Pizza");
        menuItem.ImageUrl.Should().Be("pepperoni.jpg");
        menuItem.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_Should_ReturnFailure()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();
        var restaurant = CreateTestRestaurant();

        _repositoryMock.Setup(r => r.GetByIdAsync(restaurantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(restaurant);

        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<RestaurantEntity>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var command = new AddMenuItemCommand(
            restaurantId, "Pizza", "Description", 10.99m, "Pizza");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Failed to add menu item");
    }

    private static RestaurantEntity CreateTestRestaurant()
    {
        return new RestaurantEntity(
            "Pizza Palace", "Best pizza", "123 Main St", "555-1234",
            new TimeSpan(9, 0, 0), new TimeSpan(22, 0, 0), 5.99m, 15.00m, 30);
    }
}
