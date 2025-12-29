using Common.Domain;
using FluentAssertions;

namespace Common.Domain.UnitTests;

public class EntityTests
{
    private class TestEntity : Entity<Guid>
    {
        public string Name { get; set; } = string.Empty;

        public TestEntity(Guid id, string name) : base(id)
        {
            Name = name;
        }

        private TestEntity() : base() { }
    }

    [Fact]
    public void Constructor_WithId_ShouldSetIdAndTimestamps()
    {
        // Arrange
        var id = Guid.NewGuid();
        var beforeCreation = DateTime.UtcNow;

        // Act
        var entity = new TestEntity(id, "Test");
        var afterCreation = DateTime.UtcNow;

        // Assert
        entity.Id.Should().Be(id);
        entity.CreatedAt.Should().BeOnOrAfter(beforeCreation).And.BeOnOrBefore(afterCreation);
        entity.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void Constructor_Parameterless_ShouldSetCreatedAt()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var entity = Activator.CreateInstance(typeof(TestEntity), nonPublic: true) as TestEntity;
        var afterCreation = DateTime.UtcNow;

        // Assert
        entity!.CreatedAt.Should().BeOnOrAfter(beforeCreation).And.BeOnOrBefore(afterCreation);
        entity.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void Equals_WithSameId_ShouldReturnTrue()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id, "Name1");
        var entity2 = new TestEntity(id, "Name2");

        // Act & Assert
        entity1.Equals(entity2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentIds_ShouldReturnFalse()
    {
        // Arrange
        var entity1 = new TestEntity(Guid.NewGuid(), "Name1");
        var entity2 = new TestEntity(Guid.NewGuid(), "Name2");

        // Act & Assert
        entity1.Equals(entity2).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        // Arrange
        var entity = new TestEntity(Guid.NewGuid(), "Test");

        // Act & Assert
        entity.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void OperatorEquals_WithSameId_ShouldReturnTrue()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id, "Name1");
        var entity2 = new TestEntity(id, "Name2");

        // Act & Assert
        (entity1 == entity2).Should().BeTrue();
    }

    [Fact]
    public void OperatorEquals_WithDifferentIds_ShouldReturnFalse()
    {
        // Arrange
        var entity1 = new TestEntity(Guid.NewGuid(), "Name1");
        var entity2 = new TestEntity(Guid.NewGuid(), "Name2");

        // Act & Assert
        (entity1 == entity2).Should().BeFalse();
    }

    [Fact]
    public void OperatorNotEquals_WithSameId_ShouldReturnFalse()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id, "Name1");
        var entity2 = new TestEntity(id, "Name2");

        // Act & Assert
        (entity1 != entity2).Should().BeFalse();
    }

    [Fact]
    public void OperatorNotEquals_WithDifferentIds_ShouldReturnTrue()
    {
        // Arrange
        var entity1 = new TestEntity(Guid.NewGuid(), "Name1");
        var entity2 = new TestEntity(Guid.NewGuid(), "Name2");

        // Act & Assert
        (entity1 != entity2).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_WithSameId_ShouldReturnSameHashCode()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id, "Name1");
        var entity2 = new TestEntity(id, "Name2");

        // Act & Assert
        entity1.GetHashCode().Should().Be(entity2.GetHashCode());
    }

    [Fact]
    public void UpdatedAt_CanBeSetManually()
    {
        // Arrange
        var entity = new TestEntity(Guid.NewGuid(), "Original");
        var beforeUpdate = DateTime.UtcNow;

        // Act
        System.Threading.Thread.Sleep(10); // Ensure time difference
        var updatedTime = DateTime.UtcNow;
        
        // Use reflection to set UpdatedAt as it's protected
        var updatedAtProperty = typeof(Entity<Guid>).GetProperty("UpdatedAt");
        updatedAtProperty!.SetValue(entity, updatedTime);

        var afterUpdate = DateTime.UtcNow;

        // Assert
        entity.UpdatedAt.Should().NotBeNull();
        entity.UpdatedAt.Should().Be(updatedTime);
        entity.UpdatedAt.Should().BeOnOrAfter(beforeUpdate).And.BeOnOrBefore(afterUpdate);
    }
}
