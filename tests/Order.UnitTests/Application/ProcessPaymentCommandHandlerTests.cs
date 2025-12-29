using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Order.Application.Commands;
using Order.Application.Services;
using Order.Domain.Repositories;
using OrderEntity = Order.Domain.Entities.Order;

namespace Order.UnitTests.Application;

public class ProcessPaymentCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _repositoryMock;
    private readonly Mock<IPaymentGateway> _paymentGatewayMock;
    private readonly Mock<ILogger<ProcessPaymentCommandHandler>> _loggerMock;
    private readonly ProcessPaymentCommandHandler _handler;

    public ProcessPaymentCommandHandlerTests()
    {
        _repositoryMock = new Mock<IOrderRepository>();
        _paymentGatewayMock = new Mock<IPaymentGateway>();
        _loggerMock = new Mock<ILogger<ProcessPaymentCommandHandler>>();
        _handler = new ProcessPaymentCommandHandler(
            _repositoryMock.Object,
            _paymentGatewayMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithSuccessfulPayment_Should_ConfirmPaymentAndReturnSuccess()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = CreateTestOrder();
        var transactionId = "TXN123456";

        _repositoryMock.Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _paymentGatewayMock.Setup(g => g.ProcessPaymentAsync(
                orderId,
                It.IsAny<decimal>(),
                "CreditCard",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentResult(true, transactionId, null, null));

        var command = new ProcessPaymentCommand(orderId, "CreditCard");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.TransactionId.Should().Be(transactionId);
        order.Status.Should().Be(Common.Contracts.Enums.OrderStatus.PaymentConfirmed);
        order.PaymentTransactionId.Should().Be(transactionId);
        _repositoryMock.Verify(r => r.UpdateAsync(order, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithFailedPayment_Should_MarkPaymentFailedAndReturnFailure()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = CreateTestOrder();

        _repositoryMock.Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _paymentGatewayMock.Setup(g => g.ProcessPaymentAsync(
                orderId,
                It.IsAny<decimal>(),
                "CreditCard",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentResult(false, null, "Insufficient funds", "INSUFFICIENT_FUNDS"));

        var command = new ProcessPaymentCommand(orderId, "CreditCard");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Insufficient funds");
        order.Status.Should().Be(Common.Contracts.Enums.OrderStatus.PaymentFailed);
        _repositoryMock.Verify(r => r.UpdateAsync(order, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenOrderNotFound_Should_ReturnFailure()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderEntity?)null);

        var command = new ProcessPaymentCommand(orderId, "CreditCard");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
        _paymentGatewayMock.Verify(
            g => g.ProcessPaymentAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static OrderEntity CreateTestOrder()
    {
        var order = new OrderEntity(Guid.NewGuid(), Guid.NewGuid(), "Test Address");
        order.AddItem(Guid.NewGuid(), "Test Item", 1, 10.00m);
        return order;
    }
}
