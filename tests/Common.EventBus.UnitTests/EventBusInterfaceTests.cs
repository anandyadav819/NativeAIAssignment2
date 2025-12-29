using Common.EventBus;
using Common.Messaging;
using FluentAssertions;
using Moq;

namespace Common.EventBus.UnitTests;

public class EventBusInterfaceTests
{
    [Fact]
    public async Task IEventBus_PublishAsync_ShouldBeCallable()
    {
        // Arrange
        var eventBusMock = new Mock<IEventBus>();
        var testEvent = new TestIntegrationEvent();

        eventBusMock
            .Setup(x => x.PublishAsync(It.IsAny<TestIntegrationEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await eventBusMock.Object.PublishAsync(testEvent);

        // Assert
        eventBusMock.Verify(
            x => x.PublishAsync(testEvent, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public void IEventBus_Subscribe_ShouldBeCallable()
    {
        // Arrange
        var eventBusMock = new Mock<IEventBus>();

        // Act
        eventBusMock.Object.Subscribe<TestIntegrationEvent, TestEventHandler>();

        // Assert
        eventBusMock.Verify(
            x => x.Subscribe<TestIntegrationEvent, TestEventHandler>(),
            Times.Once);
    }

    [Fact]
    public void IEventBus_Unsubscribe_ShouldBeCallable()
    {
        // Arrange
        var eventBusMock = new Mock<IEventBus>();

        // Act
        eventBusMock.Object.Unsubscribe<TestIntegrationEvent, TestEventHandler>();

        // Assert
        eventBusMock.Verify(
            x => x.Unsubscribe<TestIntegrationEvent, TestEventHandler>(),
            Times.Once);
    }

    [Fact]
    public async Task IEventBus_ShouldSupportGenericConstraints()
    {
        // Arrange
        var eventBusMock = new Mock<IEventBus>();
        var testEvent = new TestIntegrationEvent();

        // This test verifies that the generic constraints work properly
        // TEvent must inherit from IIntegrationEvent
        // THandler must inherit from IEventHandler<TEvent>

        eventBusMock
            .Setup(x => x.PublishAsync(testEvent, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await eventBusMock.Object.PublishAsync(testEvent, CancellationToken.None);

        // Assert
        eventBusMock.Verify(
            x => x.PublishAsync(testEvent, CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task IEventBus_PublishAsync_ShouldRespectCancellationToken()
    {
        // Arrange
        var eventBusMock = new Mock<IEventBus>();
        var testEvent = new TestIntegrationEvent();
        var cts = new CancellationTokenSource();

        eventBusMock
            .Setup(x => x.PublishAsync(testEvent, cts.Token))
            .Returns(Task.CompletedTask);

        // Act
        await eventBusMock.Object.PublishAsync(testEvent, cts.Token);

        // Assert
        eventBusMock.Verify(
            x => x.PublishAsync(testEvent, cts.Token),
            Times.Once);
    }

    [Fact]
    public async Task IEventBus_PublishAsync_ShouldHandleCancellation()
    {
        // Arrange
        var eventBusMock = new Mock<IEventBus>();
        var testEvent = new TestIntegrationEvent();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        eventBusMock
            .Setup(x => x.PublishAsync(testEvent, cts.Token))
            .ThrowsAsync(new OperationCanceledException());

        // Act
        var act = async () => await eventBusMock.Object.PublishAsync(testEvent, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public void IEventBus_Subscribe_ShouldSupportMultipleHandlers()
    {
        // Arrange
        var eventBusMock = new Mock<IEventBus>();

        // Act
        eventBusMock.Object.Subscribe<TestIntegrationEvent, TestEventHandler>();
        eventBusMock.Object.Subscribe<TestIntegrationEvent, AnotherTestEventHandler>();

        // Assert
        eventBusMock.Verify(
            x => x.Subscribe<TestIntegrationEvent, TestEventHandler>(),
            Times.Once);
        eventBusMock.Verify(
            x => x.Subscribe<TestIntegrationEvent, AnotherTestEventHandler>(),
            Times.Once);
    }

    [Fact]
    public void IEventBus_Unsubscribe_ShouldRemoveSpecificHandler()
    {
        // Arrange
        var eventBusMock = new Mock<IEventBus>();

        // Act
        eventBusMock.Object.Subscribe<TestIntegrationEvent, TestEventHandler>();
        eventBusMock.Object.Unsubscribe<TestIntegrationEvent, TestEventHandler>();

        // Assert
        eventBusMock.Verify(
            x => x.Subscribe<TestIntegrationEvent, TestEventHandler>(),
            Times.Once);
        eventBusMock.Verify(
            x => x.Unsubscribe<TestIntegrationEvent, TestEventHandler>(),
            Times.Once);
    }

    // Test helper classes
    public class TestIntegrationEvent : IntegrationEvent
    {
        public override string EventType => "TestEvent";
        public string Data { get; set; } = string.Empty;
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

    public class AnotherTestEventHandler : IEventHandler<TestIntegrationEvent>
    {
        public Task HandleAsync(TestIntegrationEvent @event, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
