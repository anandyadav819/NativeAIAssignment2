using FluentAssertions;
using FluentValidation.TestHelper;
using Tracking.Application.Commands;

namespace Tracking.UnitTests.Application;

public class CommandValidatorTests
{
    [Fact]
    public void UpdateDriverLocationCommandValidator_WithValidCommand_PassesValidation()
    {
        // Arrange
        var validator = new UpdateDriverLocationCommandValidator();
        var command = new UpdateDriverLocationCommand(Guid.NewGuid(), 40.7128, -74.0060);

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateDriverLocationCommandValidator_WithEmptyDriverId_FailsValidation()
    {
        // Arrange
        var validator = new UpdateDriverLocationCommandValidator();
        var command = new UpdateDriverLocationCommand(Guid.Empty, 40.7128, -74.0060);

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DriverId)
            .WithErrorMessage("Driver ID is required");
    }

    [Theory]
    [InlineData(-91)]
    [InlineData(91)]
    [InlineData(-90.1)]
    [InlineData(90.1)]
    public void UpdateDriverLocationCommandValidator_WithInvalidLatitude_FailsValidation(double latitude)
    {
        // Arrange
        var validator = new UpdateDriverLocationCommandValidator();
        var command = new UpdateDriverLocationCommand(Guid.NewGuid(), latitude, -74.0060);

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Latitude)
            .WithErrorMessage("Latitude must be between -90 and 90");
    }

    [Theory]
    [InlineData(-181)]
    [InlineData(181)]
    [InlineData(-180.1)]
    [InlineData(180.1)]
    public void UpdateDriverLocationCommandValidator_WithInvalidLongitude_FailsValidation(double longitude)
    {
        // Arrange
        var validator = new UpdateDriverLocationCommandValidator();
        var command = new UpdateDriverLocationCommand(Guid.NewGuid(), 40.7128, longitude);

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Longitude)
            .WithErrorMessage("Longitude must be between -180 and 180");
    }

    [Fact]
    public void UpdateDriverLocationCommandValidator_WithBoundaryLatitudeValues_PassesValidation()
    {
        // Arrange
        var validator = new UpdateDriverLocationCommandValidator();
        
        var commandMin = new UpdateDriverLocationCommand(Guid.NewGuid(), -90, -74.0060);
        var commandMax = new UpdateDriverLocationCommand(Guid.NewGuid(), 90, -74.0060);

        // Act
        var resultMin = validator.TestValidate(commandMin);
        var resultMax = validator.TestValidate(commandMax);

        // Assert
        resultMin.ShouldNotHaveValidationErrorFor(x => x.Latitude);
        resultMax.ShouldNotHaveValidationErrorFor(x => x.Latitude);
    }

    [Fact]
    public void UpdateDriverLocationCommandValidator_WithBoundaryLongitudeValues_PassesValidation()
    {
        // Arrange
        var validator = new UpdateDriverLocationCommandValidator();
        
        var commandMin = new UpdateDriverLocationCommand(Guid.NewGuid(), 40.7128, -180);
        var commandMax = new UpdateDriverLocationCommand(Guid.NewGuid(), 40.7128, 180);

        // Act
        var resultMin = validator.TestValidate(commandMin);
        var resultMax = validator.TestValidate(commandMax);

        // Assert
        resultMin.ShouldNotHaveValidationErrorFor(x => x.Longitude);
        resultMax.ShouldNotHaveValidationErrorFor(x => x.Longitude);
    }
}
