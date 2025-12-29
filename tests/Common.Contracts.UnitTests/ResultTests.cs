using Common.Contracts.Models;
using FluentAssertions;

namespace Common.Contracts.UnitTests;

public class ResultTests
{
    [Fact]
    public void Success_ShouldCreateSuccessResult()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().BeEmpty();
    }

    [Fact]
    public void Failure_WithError_ShouldCreateFailureResult()
    {
        // Arrange
        var errorMessage = "Operation failed";

        // Act
        var result = Result.Failure(errorMessage);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(errorMessage);
    }

    [Fact]
    public void Failure_WithNullOrEmptyError_ShouldThrowException()
    {
        // Act & Assert
        Action actionNull = () => Result.Failure(null!);
        Action actionEmpty = () => Result.Failure(string.Empty);
        Action actionWhitespace = () => Result.Failure("   ");

        actionNull.Should().Throw<InvalidOperationException>();
        actionEmpty.Should().Throw<InvalidOperationException>();
        actionWhitespace.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void SuccessTyped_WithValue_ShouldCreateSuccessResult()
    {
        // Arrange
        var value = 42;

        // Act
        var result = Result.Success(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(value);
        result.Error.Should().BeEmpty();
    }

    [Fact]
    public void SuccessTyped_WithNullValue_ShouldAllowNull()
    {
        // Act
        var result = Result.Success<string?>(null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public void FailureTyped_WithError_ShouldCreateFailureResult()
    {
        // Arrange
        var errorMessage = "Operation failed";

        // Act
        var result = Result<int>.Failure(errorMessage);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(errorMessage);
    }

    [Fact]
    public void FailureTyped_AccessingValue_ShouldNotThrowException()
    {
        // Arrange
        var result = Result.Failure<int>("Error");

        // Act & Assert
        // In this implementation, Value is accessible but will be default(T)
        result.Value.Should().Be(default(int));
    }

    [Fact]
    public void TypedResult_WithComplexType_ShouldWork()
    {
        // Arrange
        var person = new { Name = "John", Age = 30 };

        // Act
        var result = Result.Success(person);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(person);
    }

    [Fact]
    public void FailureTyped_WithNullOrEmptyError_ShouldThrowException()
    {
        // Act & Assert
        Action actionNull = () => Result.Failure<int>(null!);
        Action actionEmpty = () => Result.Failure<int>(string.Empty);

        actionNull.Should().Throw<InvalidOperationException>();
        actionEmpty.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Result_ChainedOperations_ShouldWorkCorrectly()
    {
        // Arrange
        Result<int> GetNumber(bool succeed)
        {
            return succeed ? Result.Success(42) : Result.Failure<int>("Failed to get number");
        }

        // Act
        var successResult = GetNumber(true);
        var failureResult = GetNumber(false);

        // Assert
        successResult.IsSuccess.Should().BeTrue();
        successResult.Value.Should().Be(42);
        
        failureResult.IsSuccess.Should().BeFalse();
        failureResult.Error.Should().Be("Failed to get number");
    }

    [Fact]
    public void Result_WithMultipleErrorMessages_ShouldWork()
    {
        // Arrange
        var errors = new[] { "Error 1", "Error 2", "Error 3" };
        var combinedError = string.Join("; ", errors);

        // Act
        var result = Result.Failure(combinedError);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Error 1");
        result.Error.Should().Contain("Error 2");
        result.Error.Should().Contain("Error 3");
    }
}
