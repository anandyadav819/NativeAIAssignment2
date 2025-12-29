using Common.Application.Messaging;
using Microsoft.Extensions.Logging;
using Order.Application.Services;
using Order.Domain.Repositories;

namespace Order.Application.Commands;

public class ProcessPaymentCommandHandler : ICommandHandler<ProcessPaymentCommand, ProcessPaymentResult>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentGateway _paymentGateway;
    private readonly ILogger<ProcessPaymentCommandHandler> _logger;

    public ProcessPaymentCommandHandler(
        IOrderRepository orderRepository,
        IPaymentGateway paymentGateway,
        ILogger<ProcessPaymentCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _paymentGateway = paymentGateway;
        _logger = logger;
    }

    public async Task<ProcessPaymentResult> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing payment for order {OrderId}", request.OrderId);

        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
        {
            _logger.LogWarning("Order {OrderId} not found", request.OrderId);
            return new ProcessPaymentResult(false, $"Order {request.OrderId} not found");
        }

        try
        {
            // Process payment through payment gateway
            var paymentResult = await _paymentGateway.ProcessPaymentAsync(
                request.OrderId,
                order.TotalAmount,
                request.PaymentMethod,
                cancellationToken);

            if (!paymentResult.IsSuccess)
            {
                _logger.LogWarning(
                    "Payment failed for order {OrderId}: {ErrorMessage} ({ErrorCode})",
                    request.OrderId,
                    paymentResult.ErrorMessage,
                    paymentResult.ErrorCode);

                // Mark payment as failed in the order
                order.MarkPaymentFailed(paymentResult.ErrorMessage ?? "Payment gateway error");
                await _orderRepository.UpdateAsync(order, cancellationToken);

                return new ProcessPaymentResult(false, paymentResult.ErrorMessage ?? "Payment failed");
            }

            // Payment successful - confirm payment with the transaction ID from gateway
            order.ConfirmPayment(paymentResult.TransactionId!);
            await _orderRepository.UpdateAsync(order, cancellationToken);

            _logger.LogInformation(
                "Payment confirmed for order {OrderId} with transaction {TransactionId}",
                request.OrderId,
                paymentResult.TransactionId);

            return new ProcessPaymentResult(
                true, 
                "Payment processed successfully",
                paymentResult.TransactionId);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to process payment for order {OrderId}", request.OrderId);
            return new ProcessPaymentResult(false, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment for order {OrderId}", request.OrderId);
            
            // Mark payment as failed
            try
            {
                order.MarkPaymentFailed(ex.Message);
                await _orderRepository.UpdateAsync(order, cancellationToken);
            }
            catch (Exception updateEx)
            {
                _logger.LogError(updateEx, "Failed to mark payment as failed for order {OrderId}", request.OrderId);
            }

            return new ProcessPaymentResult(false, "Payment processing failed");
        }
    }
}
