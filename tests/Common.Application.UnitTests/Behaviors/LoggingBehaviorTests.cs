using Common.Application.Behaviors;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace Common.Application.UnitTests.Behaviors;

public class LoggingBehaviorTests
{
    private readonly Mock<ILogger<LoggingBehavior<TestRequest, TestResponse>>> _loggerMock;
    private readonly Mock<RequestHandlerDelegate<TestResponse>> _nextMock;

    public LoggingBehaviorTests()
    {
        _loggerMock = new Mock<ILogger<LoggingBehavior<TestRequest, TestResponse>>>();
        _nextMock = new Mock<RequestHandlerDelegate<TestResponse>>();
    }

    [Fact]
    public async Task Handle_ShouldLogRequestName_BeforeHandling()
    {
        // Arrange
        var behavior = new LoggingBehavior<TestRequest, TestResponse>(_loggerMock.Object);
        var request = new TestRequest { Name = "Test" };
        var expectedResponse = new TestResponse { Success = true };

        _nextMock.Setup(x => x()).ReturnsAsync(expectedResponse);

        // Act
        await behavior.Handle(request, _nextMock.Object, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Handling TestRequest")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldLogRequestName_AfterHandling()
    {
        // Arrange
        var behavior = new LoggingBehavior<TestRequest, TestResponse>(_loggerMock.Object);
        var request = new TestRequest { Name = "Test" };
        var expectedResponse = new TestResponse { Success = true };

        _nextMock.Setup(x => x()).ReturnsAsync(expectedResponse);

        // Act
        await behavior.Handle(request, _nextMock.Object, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Handled TestRequest")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnResponse_FromNext()
    {
        // Arrange
        var behavior = new LoggingBehavior<TestRequest, TestResponse>(_loggerMock.Object);
        var request = new TestRequest { Name = "Test" };
        var expectedResponse = new TestResponse { Success = true };

        _nextMock.Setup(x => x()).ReturnsAsync(expectedResponse);

        // Act
        var result = await behavior.Handle(request, _nextMock.Object, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResponse);
    }

    [Fact]
    public async Task Handle_ShouldCallNext_OnlyOnce()
    {
        // Arrange
        var behavior = new LoggingBehavior<TestRequest, TestResponse>(_loggerMock.Object);
        var request = new TestRequest { Name = "Test" };
        var expectedResponse = new TestResponse { Success = true };

        _nextMock.Setup(x => x()).ReturnsAsync(expectedResponse);

        // Act
        await behavior.Handle(request, _nextMock.Object, CancellationToken.None);

        // Assert
        _nextMock.Verify(x => x(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldLogError_WhenExceptionThrown()
    {
        // Arrange
        var behavior = new LoggingBehavior<TestRequest, TestResponse>(_loggerMock.Object);
        var request = new TestRequest { Name = "Test" };
        var expectedException = new InvalidOperationException("Test exception");

        _nextMock.Setup(x => x()).ThrowsAsync(expectedException);

        // Act
        var act = async () => await behavior.Handle(request, _nextMock.Object, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error handling TestRequest")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldRethrowException_AfterLogging()
    {
        // Arrange
        var behavior = new LoggingBehavior<TestRequest, TestResponse>(_loggerMock.Object);
        var request = new TestRequest { Name = "Test" };
        var expectedException = new InvalidOperationException("Test exception");

        _nextMock.Setup(x => x()).ThrowsAsync(expectedException);

        // Act
        var act = async () => await behavior.Handle(request, _nextMock.Object, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Test exception");
    }

    [Fact]
    public async Task Handle_ShouldRespectCancellationToken()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var behavior = new LoggingBehavior<TestRequest, TestResponse>(_loggerMock.Object);
        var request = new TestRequest { Name = "Test" };

        _nextMock.Setup(x => x()).ThrowsAsync(new OperationCanceledException());

        // Act
        var act = async () => await behavior.Handle(request, _nextMock.Object, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Handle_ShouldLogBothMessages_InCorrectOrder()
    {
        // Arrange
        var logSequence = new List<string>();
        
        _loggerMock.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Callback<LogLevel, EventId, object, Exception?, Delegate>((level, eventId, state, exception, formatter) =>
            {
                logSequence.Add(state.ToString()!);
            });

        var behavior = new LoggingBehavior<TestRequest, TestResponse>(_loggerMock.Object);
        var request = new TestRequest { Name = "Test" };
        var expectedResponse = new TestResponse { Success = true };

        _nextMock.Setup(x => x()).ReturnsAsync(expectedResponse);

        // Act
        await behavior.Handle(request, _nextMock.Object, CancellationToken.None);

        // Assert
        logSequence.Should().HaveCount(2);
        logSequence[0].Should().Contain("Handling");
        logSequence[1].Should().Contain("Handled");
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
