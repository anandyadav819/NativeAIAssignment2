using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Order.Application.Services;
using Order.Infrastructure.Services;

namespace Order.UnitTests.Infrastructure;

public class MockPaymentGatewayTests
{
    private readonly Mock<ILogger<MockPaymentGateway>> _loggerMock;
    private readonly MockPaymentGateway _gateway;

    public MockPaymentGatewayTests()
    {
        _loggerMock = new Mock<ILogger<MockPaymentGateway>>();
        _gateway = new MockPaymentGateway(_loggerMock.Object);
    }

    [Fact]
    public async Task ProcessPaymentAsync_WithValidRequest_Should_ReturnPaymentResult()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        const decimal amount = 100.00m;
        const string paymentMethod = "CreditCard";

        // Act
        var result = await _gateway.ProcessPaymentAsync(orderId, amount, paymentMethod);

        // Assert
        result.Should().NotBeNull();
        // Result can be success or failure due to 10% failure rate
        if (result.IsSuccess)
        {
            result.TransactionId.Should().NotBeNullOrEmpty();
            result.TransactionId.Should().StartWith("CC-"); // CreditCard prefix
            result.ErrorMessage.Should().BeNull();
            result.ErrorCode.Should().BeNull();
        }
        else
        {
            result.TransactionId.Should().BeNull();
            result.ErrorMessage.Should().NotBeNullOrEmpty();
            result.ErrorCode.Should().NotBeNullOrEmpty();
        }
    }

    [Theory]
    [InlineData("CreditCard", "CC-")]
    [InlineData("DebitCard", "DC-")]
    [InlineData("PayPal", "PP-")]
    [InlineData("Wallet", "WL-")]
    [InlineData("UPI", "UPI-")]
    public async Task ProcessPaymentAsync_WithDifferentPaymentMethods_Should_GenerateCorrectTransactionIdPrefix(
        string paymentMethod,
        string expectedPrefix)
    {
        // Arrange
        var orderId = Guid.NewGuid();
        const decimal amount = 50.00m;

        // Act - Retry up to 20 times to get a successful payment (considering 10% failure rate)
        PaymentResult? successResult = null;
        for (int i = 0; i < 20 && successResult == null; i++)
        {
            var result = await _gateway.ProcessPaymentAsync(orderId, amount, paymentMethod);
            if (result.IsSuccess)
            {
                successResult = result;
            }
        }

        // Assert
        successResult.Should().NotBeNull("Expected at least one successful payment in 20 attempts");
        successResult!.TransactionId.Should().StartWith(expectedPrefix);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task ProcessPaymentAsync_WithInvalidAmount_Should_ReturnFailure(decimal amount)
    {
        // Arrange
        var orderId = Guid.NewGuid();
        const string paymentMethod = "CreditCard";

        // Act
        var result = await _gateway.ProcessPaymentAsync(orderId, amount, paymentMethod);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.TransactionId.Should().BeNull();
        result.ErrorMessage.Should().Contain("Invalid payment amount");
        result.ErrorCode.Should().Be("INVALID_AMOUNT");
    }

    [Theory]
    [InlineData("Cash")]
    [InlineData("Bitcoin")]
    [InlineData("Check")]
    [InlineData("")]
    [InlineData("InvalidMethod")]
    public async Task ProcessPaymentAsync_WithUnsupportedPaymentMethod_Should_ReturnFailure(string paymentMethod)
    {
        // Arrange
        var orderId = Guid.NewGuid();
        const decimal amount = 100.00m;

        // Act
        var result = await _gateway.ProcessPaymentAsync(orderId, amount, paymentMethod);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.TransactionId.Should().BeNull();
        result.ErrorMessage.Should().Contain("Unsupported payment method");
        result.ErrorCode.Should().Be("UNSUPPORTED_METHOD");
    }

    [Fact]
    public async Task ProcessPaymentAsync_Should_SimulateFailuresBasedOnFailureRate()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        const decimal amount = 100.00m;
        const string paymentMethod = "CreditCard";
        const int iterations = 100;

        var successCount = 0;
        var failureCount = 0;

        // Act - Run multiple times to test failure rate
        for (int i = 0; i < iterations; i++)
        {
            var result = await _gateway.ProcessPaymentAsync(orderId, amount, paymentMethod);
            if (result.IsSuccess)
                successCount++;
            else
                failureCount++;
        }

        // Assert - Expect approximately 10% failure rate (with some tolerance)
        var failurePercentage = (double)failureCount / iterations;
        failurePercentage.Should().BeGreaterThan(0); // Some failures should occur
        failurePercentage.Should().BeLessThan(0.3); // But not too many (allowing for randomness)
        
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Processing payment")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Exactly(iterations));
    }

    [Fact]
    public async Task ProcessPaymentAsync_WhenFails_Should_ReturnValidErrorCodes()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        const decimal amount = 100.00m;
        const string paymentMethod = "CreditCard";
        var validErrorCodes = new[] 
        { 
            "INSUFFICIENT_FUNDS", 
            "CARD_DECLINED", 
            "TIMEOUT", 
            "GATEWAY_ERROR" 
        };

        // Act - Retry until we get a failure (max 100 attempts)
        PaymentResult? failureResult = null;
        for (int i = 0; i < 100 && failureResult == null; i++)
        {
            var result = await _gateway.ProcessPaymentAsync(orderId, amount, paymentMethod);
            if (!result.IsSuccess)
            {
                failureResult = result;
            }
        }

        // Assert
        failureResult.Should().NotBeNull("Expected at least one failed payment in 100 attempts");
        failureResult!.ErrorCode.Should().BeOneOf(validErrorCodes);
        failureResult.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ProcessPaymentAsync_Should_GenerateUniqueTransactionIds()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        const decimal amount = 100.00m;
        const string paymentMethod = "CreditCard";
        var transactionIds = new HashSet<string>();

        // Act - Get multiple successful transactions
        for (int i = 0; i < 50; i++)
        {
            var result = await _gateway.ProcessPaymentAsync(orderId, amount, paymentMethod);
            if (result.IsSuccess && result.TransactionId != null)
            {
                transactionIds.Add(result.TransactionId);
            }
        }

        // Assert
        transactionIds.Should().HaveCountGreaterThan(10); // Should have multiple successful payments
        transactionIds.Should().OnlyHaveUniqueItems(); // All transaction IDs should be unique
    }

    [Fact]
    public async Task RefundPaymentAsync_WithValidTransaction_Should_ReturnRefundResult()
    {
        // Arrange
        const string transactionId = "CC-123456";
        const decimal amount = 50.00m;

        // Act
        var result = await _gateway.RefundPaymentAsync(transactionId, amount);

        // Assert
        result.Should().NotBeNull();
        // Result can be success or failure due to 5% failure rate
        if (result.IsSuccess)
        {
            result.RefundId.Should().NotBeNullOrEmpty();
            result.RefundId.Should().StartWith("REF-");
            result.ErrorMessage.Should().BeNull();
        }
        else
        {
            result.RefundId.Should().BeNull();
            result.ErrorMessage.Should().NotBeNullOrEmpty();
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-50)]
    public async Task RefundPaymentAsync_WithInvalidAmount_Should_ReturnFailure(decimal amount)
    {
        // Arrange
        const string transactionId = "CC-123456";

        // Act
        var result = await _gateway.RefundPaymentAsync(transactionId, amount);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.RefundId.Should().BeNull();
        result.ErrorMessage.Should().Contain("Invalid refund amount");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public async Task RefundPaymentAsync_WithInvalidTransactionId_Should_ReturnFailure(string? transactionId)
    {
        // Arrange
        const decimal amount = 50.00m;

        // Act
        var result = await _gateway.RefundPaymentAsync(transactionId!, amount);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.RefundId.Should().BeNull();
        result.ErrorMessage.Should().Contain("Invalid transaction ID");
    }

    [Fact]
    public async Task RefundPaymentAsync_Should_GenerateUniqueRefundIds()
    {
        // Arrange
        const string transactionId = "CC-123456";
        const decimal amount = 50.00m;
        var refundIds = new HashSet<string>();

        // Act - Get multiple successful refunds
        for (int i = 0; i < 50; i++)
        {
            var result = await _gateway.RefundPaymentAsync(transactionId, amount);
            if (result.IsSuccess && result.RefundId != null)
            {
                refundIds.Add(result.RefundId);
            }
        }

        // Assert
        refundIds.Should().HaveCountGreaterThan(10); // Should have multiple successful refunds
        refundIds.Should().OnlyHaveUniqueItems(); // All refund IDs should be unique
    }

    [Fact]
    public async Task ProcessPaymentAsync_Should_HandleCancellation()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        const decimal amount = 100.00m;
        const string paymentMethod = "CreditCard";
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await _gateway.ProcessPaymentAsync(orderId, amount, paymentMethod, cts.Token));
    }

    [Fact]
    public async Task RefundPaymentAsync_Should_HandleCancellation()
    {
        // Arrange
        const string transactionId = "CC-123456";
        const decimal amount = 50.00m;
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await _gateway.RefundPaymentAsync(transactionId, amount, cts.Token));
    }
}
