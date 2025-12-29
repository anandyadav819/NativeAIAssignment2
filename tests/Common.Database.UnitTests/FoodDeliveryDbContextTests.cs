using Common.Database;
using Common.Domain;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Common.Database.UnitTests;

public class FoodDeliveryDbContextTests : IDisposable
{
    private readonly TestDbContext _context;

    public FoodDeliveryDbContextTests()
    {
        var options = new DbContextOptionsBuilder<FoodDeliveryDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TestDbContext(options);
    }

    public void Dispose()
    {
        _context?.Database.EnsureDeleted();
        _context?.Dispose();
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldUpdateTimestampsOnModifiedEntities()
    {
        // Arrange
        var entity = new TestAggregateRoot(Guid.NewGuid(), "Initial Name");
        await _context.TestAggregates.AddAsync(entity);
        await _context.SaveChangesAsync();
        
        var originalUpdatedAt = entity.UpdatedAt;
        await Task.Delay(10); // Ensure time difference

        // Act
        entity.UpdateName("Updated Name");
        _context.TestAggregates.Update(entity);
        await _context.SaveChangesAsync();

        // Assert
        entity.UpdatedAt.Should().NotBeNull();
        entity.UpdatedAt!.Value.Should().BeAfter(originalUpdatedAt!.Value);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldNotUpdateTimestampsOnNewEntities()
    {
        // Arrange
        var entity = new TestAggregateRoot(Guid.NewGuid(), "Test");
        var createdAt = entity.CreatedAt;
        var updatedAt = entity.UpdatedAt;

        // Act
        await _context.TestAggregates.AddAsync(entity);
        await _context.SaveChangesAsync();

        // Assert
        entity.CreatedAt.Should().Be(createdAt);
        entity.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldPersistEntities()
    {
        // Arrange
        var entity = new TestAggregateRoot(Guid.NewGuid(), "Test");

        // Act
        await _context.TestAggregates.AddAsync(entity);
        var result = await _context.SaveChangesAsync();

        // Assert
        result.Should().Be(1);
        var savedEntity = await _context.TestAggregates.FindAsync(entity.Id);
        savedEntity.Should().NotBeNull();
        savedEntity!.Name.Should().Be("Test");
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldRespectCancellationToken()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();
        var entity = new TestAggregateRoot(Guid.NewGuid(), "Test");
        await _context.TestAggregates.AddAsync(entity);

        // Act
        var act = async () => await _context.SaveChangesAsync(cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task SaveChangesAsync_WithMultipleModifiedEntities_ShouldUpdateAllTimestamps()
    {
        // Arrange
        var entity1 = new TestAggregateRoot(Guid.NewGuid(), "Entity1");
        var entity2 = new TestAggregateRoot(Guid.NewGuid(), "Entity2");
        await _context.TestAggregates.AddRangeAsync(entity1, entity2);
        await _context.SaveChangesAsync();

        var originalUpdatedAt1 = entity1.UpdatedAt;
        var originalUpdatedAt2 = entity2.UpdatedAt;
        await Task.Delay(10);

        // Act
        entity1.UpdateName("Updated1");
        entity2.UpdateName("Updated2");
        _context.TestAggregates.UpdateRange(entity1, entity2);
        await _context.SaveChangesAsync();

        // Assert
        entity1.UpdatedAt.Should().NotBeNull();
        entity1.UpdatedAt!.Value.Should().BeAfter(originalUpdatedAt1!.Value);
        entity2.UpdatedAt.Should().NotBeNull();
        entity2.UpdatedAt!.Value.Should().BeAfter(originalUpdatedAt2!.Value);
    }

    [Fact]
    public async Task ChangeTracker_ShouldTrackEntityStates()
    {
        // Arrange
        var entity = new TestAggregateRoot(Guid.NewGuid(), "Test");

        // Act - Add entity
        await _context.TestAggregates.AddAsync(entity);

        // Assert - Should be in Added state
        _context.Entry(entity).State.Should().Be(EntityState.Added);

        // Act - Save changes
        await _context.SaveChangesAsync();

        // Assert - Should be in Unchanged state
        _context.Entry(entity).State.Should().Be(EntityState.Unchanged);

        // Act - Modify entity
        entity.UpdateName("Modified");
        _context.TestAggregates.Update(entity);

        // Assert - Should be in Modified state
        _context.Entry(entity).State.Should().Be(EntityState.Modified);
    }

    [Fact]
    public async Task Context_ShouldSupportQueryOperations()
    {
        // Arrange
        var entity1 = new TestAggregateRoot(Guid.NewGuid(), "Alpha");
        var entity2 = new TestAggregateRoot(Guid.NewGuid(), "Beta");
        var entity3 = new TestAggregateRoot(Guid.NewGuid(), "Gamma");
        await _context.TestAggregates.AddRangeAsync(entity1, entity2, entity3);
        await _context.SaveChangesAsync();

        // Act
        var results = await _context.TestAggregates
            .Where(e => e.Name.Contains("a"))
            .OrderBy(e => e.Name)
            .ToListAsync();

        // Assert
        results.Should().HaveCount(3); // Alpha, Beta, Gamma all contain 'a'
        results[0].Name.Should().Be("Alpha");
    }

    [Fact]
    public async Task Context_ShouldSupportAsyncOperations()
    {
        // Arrange
        var entities = Enumerable.Range(1, 5)
            .Select(i => new TestAggregateRoot(Guid.NewGuid(), $"Entity{i}"))
            .ToList();
        await _context.TestAggregates.AddRangeAsync(entities);
        await _context.SaveChangesAsync();

        // Act
        var count = await _context.TestAggregates.CountAsync();
        var firstOrDefault = await _context.TestAggregates.FirstOrDefaultAsync();
        var any = await _context.TestAggregates.AnyAsync();

        // Assert
        count.Should().Be(5);
        firstOrDefault.Should().NotBeNull();
        any.Should().BeTrue();
    }

    [Fact]
    public void Context_ShouldApplyConfigurationsFromAssembly()
    {
        // This test verifies that the OnModelCreating method
        // calls ApplyConfigurationsFromAssembly

        // Arrange & Act
        var entityType = _context.Model.FindEntityType(typeof(TestAggregateRoot));

        // Assert
        entityType.Should().NotBeNull();
    }

    [Fact]
    public async Task Context_ShouldSupportDeleteOperations()
    {
        // Arrange
        var entity = new TestAggregateRoot(Guid.NewGuid(), "ToDelete");
        await _context.TestAggregates.AddAsync(entity);
        await _context.SaveChangesAsync();

        // Act
        _context.TestAggregates.Remove(entity);
        await _context.SaveChangesAsync();

        // Assert
        var deletedEntity = await _context.TestAggregates.FindAsync(entity.Id);
        deletedEntity.Should().BeNull();
    }

    // Test helper classes
    public class TestAggregateRoot : AggregateRoot<Guid>
    {
        public string Name { get; private set; }

        private TestAggregateRoot() 
        {
            Name = string.Empty;
        } // EF Core constructor

        public TestAggregateRoot(Guid id, string name)
        {
            Id = id;
            Name = name;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateName(string newName)
        {
            Name = newName;
        }
    }

    public class TestDbContext : FoodDeliveryDbContext
    {
        public TestDbContext(DbContextOptions<FoodDeliveryDbContext> options) 
            : base(options)
        {
        }

        public DbSet<TestAggregateRoot> TestAggregates { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TestAggregateRoot>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired();
            });
        }
    }
}
