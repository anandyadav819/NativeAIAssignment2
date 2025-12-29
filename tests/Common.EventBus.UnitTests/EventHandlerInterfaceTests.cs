using Common.EventBus;
using Common.Messaging;
using FluentAssertions;
using Moq;

namespace Common.EventBus.UnitTests;

public class EventHandlerInterfaceTests
{
    [Fact]
    public async Task IEventHandler_HandleAsync_ShouldProcessEvent()
    {
        // Arrange
        var handler = new TestEventHandler();
        var testEvent = new TestIntegrationEvent { Data = "test data" };

        // Act
        await handler.HandleAsync(testEvent);

        // Assert
        handler.WasHandled.Should().BeTrue();
        handler.ReceivedEvent.Should().NotBeNull();
        handler.ReceivedEvent!.Data.Should().Be("test data");
    }

    [Fact]
    public async Task IEventHandler_HandleAsync_ShouldRespectCancellationToken()
    {
        // Arrange
        var handler = new TestEventHandler();
        var testEvent = new TestIntegrationEvent();
        var cts = new CancellationTokenSource();

        // Act
        await handler.HandleAsync(testEvent, cts.Token);

        // Assert
        handler.WasHandled.Should().BeTrue();
    }

    [Fact]
    public async Task IEventHandler_HandleAsync_ShouldHandleCancellation()
    {
        // Arrange
        var handler = new CancellableEventHandler();
        var testEvent = new TestIntegrationEvent();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var act = async () => await handler.HandleAsync(testEvent, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task IEventHandler_ShouldSupportMocking()
    {
        // Arrange
        var handlerMock = new Mock<IEventHandler<TestIntegrationEvent>>();
        var testEvent = new TestIntegrationEvent();

        handlerMock
            .Setup(x => x.HandleAsync(testEvent, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await handlerMock.Object.HandleAsync(testEvent);

        // Assert
        handlerMock.Verify(
            x => x.HandleAsync(testEvent, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task IEventHandler_ShouldProcessMultipleEvents()
    {
        // Arrange
        var handler = new TestEventHandler();
        var event1 = new TestIntegrationEvent { Data = "event1" };
        var event2 = new TestIntegrationEvent { Data = "event2" };

        // Act
        await handler.HandleAsync(event1);
        await handler.HandleAsync(event2);

        // Assert
        handler.WasHandled.Should().BeTrue();
        handler.ReceivedEvent!.Data.Should().Be("event2"); // Last event processed
    }

    [Fact]
    public async Task IEventHandler_ShouldHandleExceptionsGracefully()
    {
        // Arrange
        var handler = new FaultyEventHandler();
        var testEvent = new TestIntegrationEvent();

        // Act
        var act = async () => await handler.HandleAsync(testEvent);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Handler error");
    }

    [Fact]
    public async Task IEventHandler_ShouldProcessEventWithComplexData()
    {
        // Arrange
        var handler = new ComplexEventHandler();
        var complexEvent = new ComplexIntegrationEvent
        {
            Id = 123,
            Name = "Test",
            Items = new List<string> { "item1", "item2", "item3" },
            Metadata = new Dictionary<string, object>
            {
                { "key1", "value1" },
                { "key2", 42 }
            }
        };

        // Act
        await handler.HandleAsync(complexEvent);

        // Assert
        handler.ProcessedEvent.Should().NotBeNull();
        handler.ProcessedEvent!.Id.Should().Be(123);
        handler.ProcessedEvent.Items.Should().HaveCount(3);
        handler.ProcessedEvent.Metadata.Should().ContainKey("key1");
    }

    [Fact]
    public void IEventHandler_ShouldEnforceEventTypeConstraint()
    {
        // This test verifies that the generic constraint works
        // TEvent must implement IIntegrationEvent

        // Arrange & Act
        var handler = new TestEventHandler();

        // Assert
        handler.Should().BeAssignableTo<IEventHandler<TestIntegrationEvent>>();
    }

    // Test helper classes
    public class TestIntegrationEvent : IntegrationEvent
    {
        public override string EventType => "TestEvent";
        public string Data { get; set; } = string.Empty;
    }

    public class ComplexIntegrationEvent : IntegrationEvent
    {
        public override string EventType => "ComplexEvent";
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<string> Items { get; set; } = new();
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public class TestEventHandler : IEventHandler<TestIntegrationEvent>
    {
        public bool WasHandled { get; private set; }
        public TestIntegrationEvent? ReceivedEvent { get; private set; }

        public Task HandleAsync(TestIntegrationEvent @event, CancellationToken cancellationToken = default)
        {
            WasHandled = true;
            ReceivedEvent = @event;
            return Task.CompletedTask;
        }
    }

    public class ComplexEventHandler : IEventHandler<ComplexIntegrationEvent>
    {
        public ComplexIntegrationEvent? ProcessedEvent { get; private set; }

        public Task HandleAsync(ComplexIntegrationEvent @event, CancellationToken cancellationToken = default)
        {
            ProcessedEvent = @event;
            return Task.CompletedTask;
        }
    }

    public class CancellableEventHandler : IEventHandler<TestIntegrationEvent>
    {
        public Task HandleAsync(TestIntegrationEvent @event, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }
    }

    public class FaultyEventHandler : IEventHandler<TestIntegrationEvent>
    {
        public Task HandleAsync(TestIntegrationEvent @event, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Handler error");
        }
    }
}
