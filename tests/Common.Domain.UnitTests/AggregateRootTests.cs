using Common.Domain;
using FluentAssertions;

namespace Common.Domain.UnitTests;

public class AggregateRootTests
{
    private class TestDomainEvent : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
        public string Data { get; set; } = string.Empty;
    }

    private class TestAggregateRoot : AggregateRoot<Guid>
    {
        public string Name { get; set; } = string.Empty;

        public TestAggregateRoot(Guid id, string name) : base(id)
        {
            Name = name;
        }

        public void RaiseDomainEvent(IDomainEvent domainEvent)
        {
            AddDomainEvent(domainEvent);
        }

        public void ClearEvents()
        {
            ClearDomainEvents();
        }
    }

    [Fact]
    public void Constructor_ShouldInitializeEmptyDomainEvents()
    {
        // Arrange & Act
        var aggregate = new TestAggregateRoot(Guid.NewGuid(), "Test");

        // Assert
        aggregate.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void AddDomainEvent_ShouldAddEventToCollection()
    {
        // Arrange
        var aggregate = new TestAggregateRoot(Guid.NewGuid(), "Test");
        var domainEvent = new TestDomainEvent { Data = "Event1" };

        // Act
        aggregate.RaiseDomainEvent(domainEvent);

        // Assert
        aggregate.DomainEvents.Should().HaveCount(1);
        aggregate.DomainEvents.Should().Contain(domainEvent);
    }

    [Fact]
    public void AddDomainEvent_MultipleTimes_ShouldAddAllEvents()
    {
        // Arrange
        var aggregate = new TestAggregateRoot(Guid.NewGuid(), "Test");
        var event1 = new TestDomainEvent { Data = "Event1" };
        var event2 = new TestDomainEvent { Data = "Event2" };
        var event3 = new TestDomainEvent { Data = "Event3" };

        // Act
        aggregate.RaiseDomainEvent(event1);
        aggregate.RaiseDomainEvent(event2);
        aggregate.RaiseDomainEvent(event3);

        // Assert
        aggregate.DomainEvents.Should().HaveCount(3);
        aggregate.DomainEvents.Should().ContainInOrder(event1, event2, event3);
    }

    [Fact]
    public void ClearDomainEvents_ShouldRemoveAllEvents()
    {
        // Arrange
        var aggregate = new TestAggregateRoot(Guid.NewGuid(), "Test");
        aggregate.RaiseDomainEvent(new TestDomainEvent { Data = "Event1" });
        aggregate.RaiseDomainEvent(new TestDomainEvent { Data = "Event2" });

        // Act
        aggregate.ClearEvents();

        // Assert
        aggregate.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void ClearDomainEvents_WhenNoEvents_ShouldNotThrow()
    {
        // Arrange
        var aggregate = new TestAggregateRoot(Guid.NewGuid(), "Test");

        // Act
        var action = () => aggregate.ClearEvents();

        // Assert
        action.Should().NotThrow();
        aggregate.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void DomainEvents_ShouldBeReadOnly()
    {
        // Arrange
        var aggregate = new TestAggregateRoot(Guid.NewGuid(), "Test");
        var domainEvent = new TestDomainEvent { Data = "Event1" };
        aggregate.RaiseDomainEvent(domainEvent);

        // Act
        var events = aggregate.DomainEvents;

        // Assert
        events.Should().BeAssignableTo<IReadOnlyCollection<IDomainEvent>>();
    }

    [Fact]
    public void AggregateRoot_ShouldInheritFromEntity()
    {
        // Arrange
        var id = Guid.NewGuid();
        var aggregate = new TestAggregateRoot(id, "Test");

        // Assert
        aggregate.Should().BeAssignableTo<Entity<Guid>>();
        aggregate.Id.Should().Be(id);
    }

    [Fact]
    public void DomainEvents_AfterClear_CanAddNewEvents()
    {
        // Arrange
        var aggregate = new TestAggregateRoot(Guid.NewGuid(), "Test");
        aggregate.RaiseDomainEvent(new TestDomainEvent { Data = "Event1" });
        aggregate.ClearEvents();

        // Act
        var newEvent = new TestDomainEvent { Data = "Event2" };
        aggregate.RaiseDomainEvent(newEvent);

        // Assert
        aggregate.DomainEvents.Should().HaveCount(1);
        aggregate.DomainEvents.Should().Contain(newEvent);
    }

    [Fact]
    public void DomainEvents_ShouldPreserveOrder()
    {
        // Arrange
        var aggregate = new TestAggregateRoot(Guid.NewGuid(), "Test");
        var events = Enumerable.Range(1, 10)
            .Select(i => new TestDomainEvent { Data = $"Event{i}" })
            .ToList();

        // Act
        foreach (var evt in events)
        {
            aggregate.RaiseDomainEvent(evt);
        }

        // Assert
        aggregate.DomainEvents.Should().HaveCount(10);
        aggregate.DomainEvents.Should().ContainInOrder(events);
    }
}
