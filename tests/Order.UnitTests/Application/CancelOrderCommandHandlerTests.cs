using Common.Contracts.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Order.Application.Commands;
using Order.Domain.Repositories;
using OrderEntity = Order.Domain.Entities.Order;

namespace Order.UnitTests.Application;

public class CancelOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _repositoryMock;
    private readonly Mock<ILogger<CancelOrderCommandHandler>> _loggerMock;
    private readonly CancelOrderCommandHandler _handler;

    public CancelOrderCommandHandlerTests()
    {
        _repositoryMock = new Mock<IOrderRepository>();
        _loggerMock = new Mock<ILogger<CancelOrderCommandHandler>>();
        _handler = new CancelOrderCommandHandler(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidOrder_Should_CancelOrderAndReturnSuccess()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = CreateTestOrder();
        var reason = "Customer requested cancellation";

        _repositoryMock.Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var command = new CancelOrderCommand(orderId, reason);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("cancelled successfully");
        order.Status.Should().Be(OrderStatus.Cancelled);
        _repositoryMock.Verify(r => r.UpdateAsync(order, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenOrderNotFound_Should_ReturnFailure()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderEntity?)null);

        var command = new CancelOrderCommand(orderId, "Not important");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<OrderEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static OrderEntity CreateTestOrder()
    {
        var order = new OrderEntity(Guid.NewGuid(), Guid.NewGuid(), "Test Address");
        order.AddItem(Guid.NewGuid(), "Test Item", 1, 10.00m);
        return order;
    }
}
