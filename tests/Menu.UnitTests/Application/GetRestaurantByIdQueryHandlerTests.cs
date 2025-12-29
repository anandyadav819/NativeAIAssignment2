using Common.Caching;
using FluentAssertions;
using Menu.Application.DTOs;
using Menu.Application.Queries;
using Menu.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using RestaurantEntity = Menu.Domain.Entities.Restaurant;

namespace Menu.UnitTests.Application;

public class GetRestaurantByIdQueryHandlerTests
{
    private readonly Mock<IRestaurantRepository> _repositoryMock;
    private readonly Mock<ICacheService> _cacheMock;
    private readonly Mock<ILogger<GetRestaurantByIdQueryHandler>> _loggerMock;
    private readonly GetRestaurantByIdQueryHandler _handler;

    public GetRestaurantByIdQueryHandlerTests()
    {
        _repositoryMock = new Mock<IRestaurantRepository>();
        _cacheMock = new Mock<ICacheService>();
        _loggerMock = new Mock<ILogger<GetRestaurantByIdQueryHandler>>();
        _handler = new GetRestaurantByIdQueryHandler(
            _repositoryMock.Object,
            _cacheMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenRestaurantExists_Should_ReturnRestaurantDto()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();
        var restaurant = CreateTestRestaurant();
        restaurant.AddMenuItem("Pizza", "Delicious", 12.99m, "Pizza");

        _cacheMock.Setup(c => c.GetOrCreateAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<RestaurantDto?>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .Returns<string, Func<Task<RestaurantDto?>>, TimeSpan?, CancellationToken>(
                async (_, factory, _, _) => await factory());

        _repositoryMock.Setup(r => r.GetByIdAsync(restaurantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(restaurant);

        var query = new GetRestaurantByIdQuery(restaurantId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(restaurant.Id);
        result.Name.Should().Be(restaurant.Name);
        result.MenuItems.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_WhenRestaurantNotFound_Should_ReturnNull()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();

        _cacheMock.Setup(c => c.GetOrCreateAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<RestaurantDto?>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .Returns<string, Func<Task<RestaurantDto?>>, TimeSpan?, CancellationToken>(
                async (_, factory, _, _) => await factory());

        _repositoryMock.Setup(r => r.GetByIdAsync(restaurantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RestaurantEntity?)null);

        var query = new GetRestaurantByIdQuery(restaurantId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    private static RestaurantEntity CreateTestRestaurant()
    {
        return new RestaurantEntity(
            "Pizza Palace", "Best pizza", "123 Main St", "555-1234",
            new TimeSpan(9, 0, 0), new TimeSpan(22, 0, 0), 5.99m, 15.00m, 30);
    }
}
