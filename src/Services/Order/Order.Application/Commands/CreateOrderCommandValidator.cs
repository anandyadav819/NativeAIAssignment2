using FluentValidation;

namespace Order.Application.Commands;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required");

        RuleFor(x => x.RestaurantId)
            .NotEmpty().WithMessage("Restaurant ID is required");

        RuleFor(x => x.DeliveryAddress)
            .NotEmpty().WithMessage("Delivery address is required")
            .MaximumLength(500).WithMessage("Delivery address must not exceed 500 characters");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Order must have at least one item");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.MenuItemId)
                .NotEmpty().WithMessage("Menu item ID is required");

            item.RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Item name is required")
                .MaximumLength(200).WithMessage("Item name must not exceed 200 characters");

            item.RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero")
                .LessThanOrEqualTo(100).WithMessage("Quantity cannot exceed 100");

            item.RuleFor(x => x.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Unit price cannot be negative");
        });
    }
}
