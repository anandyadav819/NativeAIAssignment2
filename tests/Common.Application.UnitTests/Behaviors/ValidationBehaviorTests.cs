using Common.Application.Behaviors;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace Common.Application.UnitTests.Behaviors;

public class ValidationBehaviorTests
{
    private readonly Mock<ILogger<ValidationBehavior<TestRequest, TestResponse>>> _loggerMock;
    private readonly Mock<RequestHandlerDelegate<TestResponse>> _nextMock;

    public ValidationBehaviorTests()
    {
        _loggerMock = new Mock<ILogger<ValidationBehavior<TestRequest, TestResponse>>>();
        _nextMock = new Mock<RequestHandlerDelegate<TestResponse>>();
    }

    [Fact]
    public async Task Handle_ShouldCallNext_WhenNoValidators()
    {
        // Arrange
        var validators = Enumerable.Empty<IValidator<TestRequest>>();
        var behavior = new ValidationBehavior<TestRequest, TestResponse>(validators, _loggerMock.Object);
        var request = new TestRequest { Name = "Test" };
        var expectedResponse = new TestResponse { Success = true };

        _nextMock.Setup(x => x()).ReturnsAsync(expectedResponse);

        // Act
        var result = await behavior.Handle(request, _nextMock.Object, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResponse);
        _nextMock.Verify(x => x(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCallNext_WhenValidationSucceeds()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<TestRequest>>();
        validatorMock
            .Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var validators = new[] { validatorMock.Object };
        var behavior = new ValidationBehavior<TestRequest, TestResponse>(validators, _loggerMock.Object);
        var request = new TestRequest { Name = "Test" };
        var expectedResponse = new TestResponse { Success = true };

        _nextMock.Setup(x => x()).ReturnsAsync(expectedResponse);

        // Act
        var result = await behavior.Handle(request, _nextMock.Object, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResponse);
        _nextMock.Verify(x => x(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenValidationFails()
    {
        // Arrange
        var validationFailure = new ValidationFailure("Name", "Name is required");
        var validationResult = new ValidationResult(new[] { validationFailure });

        var validatorMock = new Mock<IValidator<TestRequest>>();
        validatorMock
            .Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        var validators = new[] { validatorMock.Object };
        var behavior = new ValidationBehavior<TestRequest, TestResponse>(validators, _loggerMock.Object);
        var request = new TestRequest { Name = "" };

        // Act
        var act = async () => await behavior.Handle(request, _nextMock.Object, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .Where(ex => ex.Errors.Any(e => e.ErrorMessage == "Name is required"));
        _nextMock.Verify(x => x(), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WithMultipleFailures()
    {
        // Arrange
        var validationFailures = new[]
        {
            new ValidationFailure("Name", "Name is required"),
            new ValidationFailure("Email", "Email is invalid")
        };
        var validationResult = new ValidationResult(validationFailures);

        var validatorMock = new Mock<IValidator<TestRequest>>();
        validatorMock
            .Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        var validators = new[] { validatorMock.Object };
        var behavior = new ValidationBehavior<TestRequest, TestResponse>(validators, _loggerMock.Object);
        var request = new TestRequest { Name = "" };

        // Act
        var act = async () => await behavior.Handle(request, _nextMock.Object, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .Where(ex => ex.Errors.Count() == 2);
        _nextMock.Verify(x => x(), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldAggregateFailuresFromMultipleValidators()
    {
        // Arrange
        var validator1Mock = new Mock<IValidator<TestRequest>>();
        validator1Mock
            .Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("Name", "Name is required") }));

        var validator2Mock = new Mock<IValidator<TestRequest>>();
        validator2Mock
            .Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("Email", "Email is invalid") }));

        var validators = new[] { validator1Mock.Object, validator2Mock.Object };
        var behavior = new ValidationBehavior<TestRequest, TestResponse>(validators, _loggerMock.Object);
        var request = new TestRequest { Name = "" };

        // Act
        var act = async () => await behavior.Handle(request, _nextMock.Object, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .Where(ex => ex.Errors.Count() == 2);
    }

    [Fact]
    public async Task Handle_ShouldLogWarning_WhenValidationFails()
    {
        // Arrange
        var validationFailure = new ValidationFailure("Name", "Name is required");
        var validationResult = new ValidationResult(new[] { validationFailure });

        var validatorMock = new Mock<IValidator<TestRequest>>();
        validatorMock
            .Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        var validators = new[] { validatorMock.Object };
        var behavior = new ValidationBehavior<TestRequest, TestResponse>(validators, _loggerMock.Object);
        var request = new TestRequest { Name = "" };

        // Act
        try
        {
            await behavior.Handle(request, _nextMock.Object, CancellationToken.None);
        }
        catch (ValidationException)
        {
            // Expected
        }

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Validation failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldRespectCancellationToken()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var validatorMock = new Mock<IValidator<TestRequest>>();
        validatorMock
            .Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var validators = new[] { validatorMock.Object };
        var behavior = new ValidationBehavior<TestRequest, TestResponse>(validators, _loggerMock.Object);
        var request = new TestRequest { Name = "Test" };

        // Act
        var act = async () => await behavior.Handle(request, _nextMock.Object, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    // Test helper classes
    public class TestRequest : IRequest<TestResponse>
    {
        public string Name { get; set; } = string.Empty;
    }

    public class TestResponse
    {
        public bool Success { get; set; }
    }
}
