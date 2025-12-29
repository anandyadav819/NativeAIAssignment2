using Common.Application.Behaviors;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace Common.Application.UnitTests.Behaviors;

public class PerformanceBehaviorTests
{
    private readonly Mock<ILogger<PerformanceBehavior<TestRequest, TestResponse>>> _loggerMock;
    private readonly Mock<RequestHandlerDelegate<TestResponse>> _nextMock;

    public PerformanceBehaviorTests()
    {
        _loggerMock = new Mock<ILogger<PerformanceBehavior<TestRequest, TestResponse>>>();
        _nextMock = new Mock<RequestHandlerDelegate<TestResponse>>();
    }

    [Fact]
    public async Task Handle_ShouldNotLogWarning_WhenRequestCompletesQuickly()
    {
        // Arrange
        var behavior = new PerformanceBehavior<TestRequest, TestResponse>(_loggerMock.Object);
        var request = new TestRequest { Name = "Test" };
        var expectedResponse = new TestResponse { Success = true };

        _nextMock.Setup(x => x()).ReturnsAsync(expectedResponse);

        // Act
        await behavior.Handle(request, _nextMock.Object, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldLogWarning_WhenRequestTakesLongerThan500Ms()
    {
        // Arrange
        var behavior = new PerformanceBehavior<TestRequest, TestResponse>(_loggerMock.Object);
        var request = new TestRequest { Name = "Test" };
        var expectedResponse = new TestResponse { Success = true };

        _nextMock.Setup(x => x()).ReturnsAsync(() =>
        {
            Thread.Sleep(600); // Simulate slow operation
            return expectedResponse;
        });

        // Act
        await behavior.Handle(request, _nextMock.Object, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Long Running Request")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldIncludeRequestName_InWarningLog()
    {
        // Arrange
        var behavior = new PerformanceBehavior<TestRequest, TestResponse>(_loggerMock.Object);
        var request = new TestRequest { Name = "Test" };
        var expectedResponse = new TestResponse { Success = true };

        _nextMock.Setup(x => x()).ReturnsAsync(() =>
        {
            Thread.Sleep(600);
            return expectedResponse;
        });

        // Act
        await behavior.Handle(request, _nextMock.Object, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("TestRequest")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldIncludeElapsedTime_InWarningLog()
    {
        // Arrange
        var behavior = new PerformanceBehavior<TestRequest, TestResponse>(_loggerMock.Object);
        var request = new TestRequest { Name = "Test" };
        var expectedResponse = new TestResponse { Success = true };

        _nextMock.Setup(x => x()).ReturnsAsync(() =>
        {
            Thread.Sleep(600);
            return expectedResponse;
        });

        // Act
        await behavior.Handle(request, _nextMock.Object, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("milliseconds")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnResponse_FromNext()
    {
        // Arrange
        var behavior = new PerformanceBehavior<TestRequest, TestResponse>(_loggerMock.Object);
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
        var behavior = new PerformanceBehavior<TestRequest, TestResponse>(_loggerMock.Object);
        var request = new TestRequest { Name = "Test" };
        var expectedResponse = new TestResponse { Success = true };

        _nextMock.Setup(x => x()).ReturnsAsync(expectedResponse);

        // Act
        await behavior.Handle(request, _nextMock.Object, CancellationToken.None);

        // Assert
        _nextMock.Verify(x => x(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldMeasurePerformance_EvenWhenExceptionThrown()
    {
        // Arrange
        var behavior = new PerformanceBehavior<TestRequest, TestResponse>(_loggerMock.Object);
        var request = new TestRequest { Name = "Test" };

        _nextMock.Setup(x => x()).ThrowsAsync(new InvalidOperationException("Test exception"));

        // Act
        var act = async () => await behavior.Handle(request, _nextMock.Object, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_ShouldRespectCancellationToken()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var behavior = new PerformanceBehavior<TestRequest, TestResponse>(_loggerMock.Object);
        var request = new TestRequest { Name = "Test" };

        _nextMock.Setup(x => x()).ThrowsAsync(new OperationCanceledException());

        // Act
        var act = async () => await behavior.Handle(request, _nextMock.Object, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Handle_ShouldNotLogWarning_WhenBelowThreshold()
    {
        // Arrange
        var behavior = new PerformanceBehavior<TestRequest, TestResponse>(_loggerMock.Object);
        var request = new TestRequest { Name = "Test" };
        var expectedResponse = new TestResponse { Success = true };

        _nextMock.Setup(x => x()).ReturnsAsync(() =>
        {
            Thread.Sleep(100); // Well below threshold
            return expectedResponse;
        });

        // Act
        await behavior.Handle(request, _nextMock.Object, CancellationToken.None);

        // Assert - Should not log when below threshold
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnResponse_EvenWhenSlowRequest()
    {
        // Arrange
        var behavior = new PerformanceBehavior<TestRequest, TestResponse>(_loggerMock.Object);
        var request = new TestRequest { Name = "Test" };
        var expectedResponse = new TestResponse { Success = true };

        _nextMock.Setup(x => x()).ReturnsAsync(() =>
        {
            Thread.Sleep(600);
            return expectedResponse;
        });

        // Act
        var result = await behavior.Handle(request, _nextMock.Object, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResponse);
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
