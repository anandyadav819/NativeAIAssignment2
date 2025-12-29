using Common.Domain.Exceptions;
using FluentAssertions;

namespace Common.Domain.UnitTests;

public class DomainExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_ShouldSetMessage()
    {
        // Arrange
        var message = "Domain exception occurred";

        // Act
        var exception = new DomainException(message);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_ShouldSetBoth()
    {
        // Arrange
        var message = "Domain exception occurred";
        var innerException = new InvalidOperationException("Inner error");

        // Act
        var exception = new DomainException(message, innerException);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().Be(innerException);
    }

    [Fact]
    public void DomainException_ShouldBeThrowable()
    {
        // Arrange
        var message = "Test exception";

        // Act
        Action action = () => throw new DomainException(message);

        // Assert
        action.Should().Throw<DomainException>()
            .WithMessage(message);
    }

    [Fact]
    public void DomainException_ShouldInheritFromException()
    {
        // Arrange
        var exception = new DomainException("Test");

        // Assert
        exception.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void DomainException_WithEmptyMessage_ShouldAccept()
    {
        // Act
        var exception = new DomainException(string.Empty);

        // Assert
        exception.Message.Should().BeEmpty();
    }

    [Fact]
    public void DomainException_CanBeCaught_AsGeneralException()
    {
        // Arrange
        var caught = false;

        // Act
        try
        {
            throw new DomainException("Test");
        }
        catch (Exception)
        {
            caught = true;
        }

        // Assert
        caught.Should().BeTrue();
    }
}
