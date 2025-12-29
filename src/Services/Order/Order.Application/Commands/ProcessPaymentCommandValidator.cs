using FluentValidation;

namespace Order.Application.Commands;

public class ProcessPaymentCommandValidator : AbstractValidator<ProcessPaymentCommand>
{
    public ProcessPaymentCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("Order ID is required");

        RuleFor(x => x.PaymentMethod)
            .NotEmpty()
            .WithMessage("Payment method is required")
            .MaximumLength(50)
            .WithMessage("Payment method must not exceed 50 characters")
            .Must(method => new[] { "CreditCard", "DebitCard", "PayPal", "Wallet", "UPI" }
                .Contains(method, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Payment method must be one of: CreditCard, DebitCard, PayPal, Wallet, UPI");
    }
}
