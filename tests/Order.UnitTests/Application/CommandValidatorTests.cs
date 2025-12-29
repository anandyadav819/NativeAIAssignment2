using FluentAssertions;
using FluentValidation.TestHelper;
using Order.Application.Commands;

namespace Order.UnitTests.Application;

public class CommandValidatorTests
{
    [Theory]
    [InlineData("CreditCard")]
    [InlineData("DebitCard")]
    [InlineData("PayPal")]
    [InlineData("Wallet")]
    [InlineData("UPI")]
    public void ProcessPaymentCommandValidator_WithValidPaymentMethod_Should_Pass(string paymentMethod)
    {
        // Arrange
        var validator = new ProcessPaymentCommandValidator();
        var command = new ProcessPaymentCommand(Guid.NewGuid(), paymentMethod);

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("Cash")]
    [InlineData("Invalid")]
    public void ProcessPaymentCommandValidator_WithInvalidPaymentMethod_Should_Fail(string paymentMethod)
    {
        // Arrange
        var validator = new ProcessPaymentCommandValidator();
        var command = new ProcessPaymentCommand(Guid.NewGuid(), paymentMethod);

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.PaymentMethod);
    }

    [Fact]
    public void CancelOrderCommandValidator_WithValidCommand_Should_Pass()
    {
        // Arrange
        var validator = new CancelOrderCommandValidator();
        var command = new CancelOrderCommand(Guid.NewGuid(), "Customer requested cancellation");

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CancelOrderCommandValidator_WithEmptyReason_Should_Fail()
    {
        // Arrange
        var validator = new CancelOrderCommandValidator();
        var command = new CancelOrderCommand(Guid.NewGuid(), "");

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Reason);
    }

    [Fact]
    public void AssignDriverCommandValidator_WithValidCommand_Should_Pass()
    {
        // Arrange
        var validator = new AssignDriverCommandValidator();
        var command = new AssignDriverCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void AssignDriverCommandValidator_WithEmptyOrderId_Should_Fail()
    {
        // Arrange
        var validator = new AssignDriverCommandValidator();
        var command = new AssignDriverCommand(Guid.Empty, Guid.NewGuid());

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.OrderId);
    }

    [Fact]
    public void CreateOrderCommandValidator_WithValidCommand_Should_Pass()
    {
        // Arrange
        var validator = new CreateOrderCommandValidator();
        var command = new CreateOrderCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "123 Main St",
            new List<OrderItemCommand>
            {
                new(Guid.NewGuid(), "Pizza", 1, 10.00m)
            });

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateOrderCommandValidator_WithEmptyItems_Should_Fail()
    {
        // Arrange
        var validator = new CreateOrderCommandValidator();
        var command = new CreateOrderCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "123 Main St",
            new List<OrderItemCommand>());

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Items);
    }
}
