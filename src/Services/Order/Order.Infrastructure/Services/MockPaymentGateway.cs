using Microsoft.Extensions.Logging;
using Order.Application.Services;

namespace Order.Infrastructure.Services;

/// <summary>
/// Mock payment gateway for testing and development
/// Simulates various payment scenarios including success, failure, and delays
/// </summary>
public class MockPaymentGateway : IPaymentGateway
{
    private readonly ILogger<MockPaymentGateway> _logger;
    private readonly Random _random = new();

    // Configurable failure rate (0.0 to 1.0)
    private const double FailureRate = 0.1; // 10% failure rate

    public MockPaymentGateway(ILogger<MockPaymentGateway> logger)
    {
        _logger = logger;
    }

    public async Task<PaymentResult> ProcessPaymentAsync(
        Guid orderId,
        decimal amount,
        string paymentMethod,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "MockPaymentGateway: Processing payment for order {OrderId}, Amount: {Amount}, Method: {PaymentMethod}",
            orderId,
            amount,
            paymentMethod);

        // Simulate network delay
        await Task.Delay(_random.Next(100, 500), cancellationToken);

        // Validate amount
        if (amount <= 0)
        {
            _logger.LogWarning("Payment failed: Invalid amount {Amount}", amount);
            return new PaymentResult(
                IsSuccess: false,
                TransactionId: null,
                ErrorMessage: "Invalid payment amount",
                ErrorCode: "INVALID_AMOUNT");
        }

        // Simulate payment method validation
        var validMethods = new[] { "CreditCard", "DebitCard", "PayPal", "Wallet", "UPI" };
        if (!validMethods.Contains(paymentMethod, StringComparer.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Payment failed: Unsupported payment method {PaymentMethod}", paymentMethod);
            return new PaymentResult(
                IsSuccess: false,
                TransactionId: null,
                ErrorMessage: $"Unsupported payment method: {paymentMethod}",
                ErrorCode: "UNSUPPORTED_METHOD");
        }

        // Simulate random payment failures (e.g., insufficient funds, card declined)
        if (_random.NextDouble() < FailureRate)
        {
            var errorScenarios = new[]
            {
                ("Insufficient funds", "INSUFFICIENT_FUNDS"),
                ("Card declined by bank", "CARD_DECLINED"),
                ("Transaction timeout", "TIMEOUT"),
                ("Payment gateway error", "GATEWAY_ERROR")
            };

            var (errorMessage, errorCode) = errorScenarios[_random.Next(errorScenarios.Length)];
            
            _logger.LogWarning(
                "Payment failed for order {OrderId}: {ErrorMessage}",
                orderId,
                errorMessage);

            return new PaymentResult(
                IsSuccess: false,
                TransactionId: null,
                ErrorMessage: errorMessage,
                ErrorCode: errorCode);
        }

        // Simulate successful payment
        var transactionId = GenerateTransactionId(paymentMethod);
        
        _logger.LogInformation(
            "Payment successful for order {OrderId}, Transaction ID: {TransactionId}",
            orderId,
            transactionId);

        return new PaymentResult(
            IsSuccess: true,
            TransactionId: transactionId,
            ErrorMessage: null,
            ErrorCode: null);
    }

    public async Task<RefundResult> RefundPaymentAsync(
        string transactionId,
        decimal amount,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "MockPaymentGateway: Processing refund for transaction {TransactionId}, Amount: {Amount}",
            transactionId,
            amount);

        // Simulate network delay
        await Task.Delay(_random.Next(100, 300), cancellationToken);

        // Validate transaction ID
        if (string.IsNullOrWhiteSpace(transactionId))
        {
            _logger.LogWarning("Refund failed: Invalid transaction ID");
            return new RefundResult(
                IsSuccess: false,
                RefundId: null,
                ErrorMessage: "Invalid transaction ID");
        }

        // Validate amount
        if (amount <= 0)
        {
            _logger.LogWarning("Refund failed: Invalid amount {Amount}", amount);
            return new RefundResult(
                IsSuccess: false,
                RefundId: null,
                ErrorMessage: "Invalid refund amount");
        }

        // Simulate occasional refund failures (5% failure rate)
        if (_random.NextDouble() < 0.05)
        {
            _logger.LogWarning("Refund failed for transaction {TransactionId}", transactionId);
            return new RefundResult(
                IsSuccess: false,
                RefundId: null,
                ErrorMessage: "Refund processing failed");
        }

        // Simulate successful refund
        var refundId = $"REF-{Guid.NewGuid():N}";
        
        _logger.LogInformation(
            "Refund successful for transaction {TransactionId}, Refund ID: {RefundId}",
            transactionId,
            refundId);

        return new RefundResult(
            IsSuccess: true,
            RefundId: refundId,
            ErrorMessage: null);
    }

    private string GenerateTransactionId(string paymentMethod)
    {
        var prefix = paymentMethod.ToUpper() switch
        {
            "CREDITCARD" => "CC",
            "DEBITCARD" => "DC",
            "PAYPAL" => "PP",
            "WALLET" => "WL",
            "UPI" => "UPI",
            _ => "TXN"
        };

        return $"{prefix}-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}";
    }
}
