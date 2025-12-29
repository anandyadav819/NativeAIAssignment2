namespace Order.Application.Services;

/// <summary>
/// Payment gateway interface for processing payments
/// </summary>
public interface IPaymentGateway
{
    /// <summary>
    /// Process a payment transaction
    /// </summary>
    /// <param name="orderId">The order ID</param>
    /// <param name="amount">The payment amount</param>
    /// <param name="paymentMethod">The payment method (CreditCard, DebitCard, PayPal, etc.)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Payment result with transaction ID or error</returns>
    Task<PaymentResult> ProcessPaymentAsync(
        Guid orderId,
        decimal amount,
        string paymentMethod,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Refund a payment transaction
    /// </summary>
    /// <param name="transactionId">The original transaction ID</param>
    /// <param name="amount">The refund amount</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Refund result</returns>
    Task<RefundResult> RefundPaymentAsync(
        string transactionId,
        decimal amount,
        CancellationToken cancellationToken = default);
}

public record PaymentResult(
    bool IsSuccess,
    string? TransactionId,
    string? ErrorMessage,
    string? ErrorCode);

public record RefundResult(
    bool IsSuccess,
    string? RefundId,
    string? ErrorMessage);
