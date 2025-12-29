using Common.Contracts.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Order.Application.Commands;
using Order.Domain.Repositories;
using OrderEntity = Order.Domain.Entities.Order;

namespace Order.UnitTests.Application;

public class AssignDriverCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _repositoryMock;
    private readonly Mock<ILogger<AssignDriverCommandHandler>> _loggerMock;
    private readonly AssignDriverCommandHandler _handler;

    public AssignDriverCommandHandlerTests()
    {
        _repositoryMock = new Mock<IOrderRepository>();
        _loggerMock = new Mock<ILogger<AssignDriverCommandHandler>>();
        _handler = new AssignDriverCommandHandler(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidOrder_Should_AssignDriverAndReturnSuccess()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var driverId = Guid.NewGuid();
        var order = CreateReadyForPickupOrder();

        _repositoryMock.Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var command = new AssignDriverCommand(orderId, driverId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("Driver assigned successfully");
        order.DriverId.Should().Be(driverId);
        order.Status.Should().Be(OrderStatus.OutForDelivery);
        _repositoryMock.Verify(r => r.UpdateAsync(order, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenOrderNotFound_Should_ReturnFailure()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var driverId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderEntity?)null);

        var command = new AssignDriverCommand(orderId, driverId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<OrderEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static OrderEntity CreateReadyForPickupOrder()
    {
        var order = new OrderEntity(Guid.NewGuid(), Guid.NewGuid(), "Test Address");
        order.AddItem(Guid.NewGuid(), "Test Item", 1, 10.00m);
        order.ConfirmPayment("TXN123");
        order.Confirm();
        order.StartPreparing();
        order.MarkReadyForPickup();
        return order;
    }
}
