using FluentValidation;

namespace Order.Application.Commands;

public class AssignDriverCommandValidator : AbstractValidator<AssignDriverCommand>
{
    public AssignDriverCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("Order ID is required");

        RuleFor(x => x.DriverId)
            .NotEmpty()
            .WithMessage("Driver ID is required");
    }
}
