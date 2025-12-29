using Common.EventBus;
using Common.Messaging;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Order.Application.Commands;
using Order.Domain.Repositories;
using OrderEntity = Order.Domain.Entities.Order;

namespace Order.UnitTests.Application;

public class CreateOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _repositoryMock;
    private readonly Mock<IEventBus> _eventBusMock;
    private readonly Mock<ILogger<CreateOrderCommandHandler>> _loggerMock;
    private readonly CreateOrderCommandHandler _handler;

    public CreateOrderCommandHandlerTests()
    {
        _repositoryMock = new Mock<IOrderRepository>();
        _eventBusMock = new Mock<IEventBus>();
        _loggerMock = new Mock<ILogger<CreateOrderCommandHandler>>();
        _handler = new CreateOrderCommandHandler(_repositoryMock.Object, _eventBusMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_Should_CreateOrderAndReturnSuccess()
    {
        // Arrange
        var command = new CreateOrderCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "123 Main St",
            new List<OrderItemCommand>
            {
                new(Guid.NewGuid(), "Pizza", 2, 15.99m),
                new(Guid.NewGuid(), "Coke", 1, 2.50m)
            });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<OrderEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        _eventBusMock.Verify(e => e.PublishAsync(It.IsAny<IIntegrationEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithMultipleItems_Should_CalculateCorrectTotalAmount()
    {
        // Arrange
        OrderEntity? capturedOrder = null;
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<OrderEntity>(), It.IsAny<CancellationToken>()))
            .Callback<OrderEntity, CancellationToken>((order, _) => capturedOrder = order);

        var command = new CreateOrderCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "123 Main St",
            new List<OrderItemCommand>
            {
                new(Guid.NewGuid(), "Pizza", 2, 15.99m),
                new(Guid.NewGuid(), "Burger", 1, 9.99m)
            });

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedOrder.Should().NotBeNull();
        capturedOrder!.Items.Should().HaveCount(2);
        capturedOrder.TotalAmount.Should().Be(41.97m); // (2 * 15.99) + (1 * 9.99)
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_Should_ReturnFailure()
    {
        // Arrange
        var command = new CreateOrderCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "123 Main St",
            new List<OrderItemCommand>
            {
                new(Guid.NewGuid(), "Pizza", 1, 15.99m)
            });

        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<OrderEntity>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Failed to create order");
    }
}
