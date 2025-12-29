using FluentAssertions;
using FluentValidation.TestHelper;
using Menu.Application.Commands;

namespace Menu.UnitTests.Application;

public class CommandValidatorTests
{
    [Fact]
    public void CreateRestaurantCommandValidator_WithValidCommand_Should_Pass()
    {
        // Arrange
        var validator = new CreateRestaurantCommandValidator();
        var command = new CreateRestaurantCommand(
            "Pizza Palace",
            "Best pizza in town",
            "123 Main St",
            "555-1234",
            new TimeSpan(9, 0, 0),
            new TimeSpan(22, 0, 0),
            5.99m,
            15.00m,
            30);

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void CreateRestaurantCommandValidator_WithEmptyName_Should_Fail(string name)
    {
        // Arrange
        var validator = new CreateRestaurantCommandValidator();
        var command = new CreateRestaurantCommand(
            name, "Description", "Address", "Phone",
            new TimeSpan(9, 0, 0), new TimeSpan(22, 0, 0), 5.99m, 15.00m, 30);

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Name);
    }

    [Fact]
    public void CreateRestaurantCommandValidator_WithNegativeDeliveryFee_Should_Fail()
    {
        // Arrange
        var validator = new CreateRestaurantCommandValidator();
        var command = new CreateRestaurantCommand(
            "Restaurant", "Description", "Address", "Phone",
            new TimeSpan(9, 0, 0), new TimeSpan(22, 0, 0), -5.99m, 15.00m, 30);

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.DeliveryFee);
    }

    [Fact]
    public void CreateRestaurantCommandValidator_WithNegativeMinimumOrderAmount_Should_Fail()
    {
        // Arrange
        var validator = new CreateRestaurantCommandValidator();
        var command = new CreateRestaurantCommand(
            "Restaurant", "Description", "Address", "Phone",
            new TimeSpan(9, 0, 0), new TimeSpan(22, 0, 0), 5.99m, -15.00m, 30);

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.MinimumOrderAmount);
    }

    [Fact]
    public void AddMenuItemCommandValidator_WithValidCommand_Should_Pass()
    {
        // Arrange
        var validator = new AddMenuItemCommandValidator();
        var command = new AddMenuItemCommand(
            Guid.NewGuid(), "Margherita Pizza", "Classic pizza", 12.99m, "Pizza", "pizza.jpg");

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void AddMenuItemCommandValidator_WithEmptyRestaurantId_Should_Fail()
    {
        // Arrange
        var validator = new AddMenuItemCommandValidator();
        var command = new AddMenuItemCommand(
            Guid.Empty, "Pizza", "Description", 10.99m, "Category");

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.RestaurantId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void AddMenuItemCommandValidator_WithEmptyName_Should_Fail(string name)
    {
        // Arrange
        var validator = new AddMenuItemCommandValidator();
        var command = new AddMenuItemCommand(
            Guid.NewGuid(), name, "Description", 10.99m, "Category");

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Name);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10.99)]
    public void AddMenuItemCommandValidator_WithInvalidPrice_Should_Fail(decimal price)
    {
        // Arrange
        var validator = new AddMenuItemCommandValidator();
        var command = new AddMenuItemCommand(
            Guid.NewGuid(), "Pizza", "Description", price, "Category");

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Price);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void AddMenuItemCommandValidator_WithEmptyCategory_Should_Fail(string category)
    {
        // Arrange
        var validator = new AddMenuItemCommandValidator();
        var command = new AddMenuItemCommand(
            Guid.NewGuid(), "Pizza", "Description", 10.99m, category);

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Category);
    }
}
