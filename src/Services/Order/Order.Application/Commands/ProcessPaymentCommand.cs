using Common.Application.Messaging;

namespace Order.Application.Commands;

public record ProcessPaymentCommand(Guid OrderId, string PaymentMethod) 
    : ICommand<ProcessPaymentResult>;

public record ProcessPaymentResult(bool Success, string Message, string? TransactionId = null);
