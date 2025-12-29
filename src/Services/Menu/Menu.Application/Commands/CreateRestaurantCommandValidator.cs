using FluentValidation;

namespace Menu.Application.Commands;

public class CreateRestaurantCommandValidator : AbstractValidator<CreateRestaurantCommand>
{
    public CreateRestaurantCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Restaurant name is required")
            .MaximumLength(200).WithMessage("Restaurant name must not exceed 200 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required")
            .MaximumLength(500).WithMessage("Address must not exceed 500 characters");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required")
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters");

        RuleFor(x => x.DeliveryFee)
            .GreaterThanOrEqualTo(0).WithMessage("Delivery fee cannot be negative");

        RuleFor(x => x.MinimumOrderAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum order amount cannot be negative");

        RuleFor(x => x.EstimatedPreparationTimeMinutes)
            .GreaterThan(0).WithMessage("Preparation time must be greater than zero")
            .LessThanOrEqualTo(240).WithMessage("Preparation time cannot exceed 240 minutes");

        RuleFor(x => x.OpeningTime)
            .LessThan(x => x.ClosingTime).WithMessage("Opening time must be before closing time");
    }
}
