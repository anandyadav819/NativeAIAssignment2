using FluentValidation;

namespace Menu.Application.Commands;

public class AddMenuItemCommandValidator : AbstractValidator<AddMenuItemCommand>
{
    public AddMenuItemCommandValidator()
    {
        RuleFor(x => x.RestaurantId)
            .NotEmpty().WithMessage("Restaurant ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Menu item name is required")
            .MaximumLength(200).WithMessage("Menu item name must not exceed 200 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required")
            .MaximumLength(100).WithMessage("Category must not exceed 100 characters");

        RuleFor(x => x.ImageUrl)
            .MaximumLength(500).WithMessage("Image URL must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.ImageUrl));
    }
}
