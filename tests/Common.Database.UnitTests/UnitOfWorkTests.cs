using Common.Database;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;

namespace Common.Database.UnitTests;

public class UnitOfWorkTests : IDisposable
{
    private readonly TestDbContext _context;
    private readonly UnitOfWork _unitOfWork;

    public UnitOfWorkTests()
    {
        var options = new DbContextOptionsBuilder<FoodDeliveryDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _context = new TestDbContext(options);
        _unitOfWork = new UnitOfWork(_context);
    }

    public void Dispose()
    {
        _unitOfWork?.Dispose();
        _context?.Database.EnsureDeleted();
        _context?.Dispose();
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldPersistChanges()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Test" };
        await _context.TestEntities.AddAsync(entity);

        // Act
        var result = await _unitOfWork.SaveChangesAsync();

        // Assert
        result.Should().Be(1);
        var savedEntity = await _context.TestEntities.FindAsync(entity.Id);
        savedEntity.Should().NotBeNull();
        savedEntity!.Name.Should().Be("Test");
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldReturnNumberOfAffectedEntries()
    {
        // Arrange
        var entity1 = new TestEntity { Id = Guid.NewGuid(), Name = "Test1" };
        var entity2 = new TestEntity { Id = Guid.NewGuid(), Name = "Test2" };
        await _context.TestEntities.AddRangeAsync(entity1, entity2);

        // Act
        var result = await _unitOfWork.SaveChangesAsync();

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldRespectCancellationToken()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Test" };
        await _context.TestEntities.AddAsync(entity);

        // Act
        var act = async () => await _unitOfWork.SaveChangesAsync(cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task CommitTransactionAsync_WithoutBeginTransaction_ShouldThrowException()
    {
        // Act
        var act = async () => await _unitOfWork.CommitTransactionAsync();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Transaction has not been started");
    }

    [Fact]
    public async Task RollbackTransactionAsync_WithoutTransaction_ShouldNotThrow()
    {
        // Act
        var act = async () => await _unitOfWork.RollbackTransactionAsync();

        // Assert
        await act.Should().NotThrowAsync();
    }

    // Test helper classes
    public class TestEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class TestDbContext : FoodDeliveryDbContext
    {
        public TestDbContext(DbContextOptions<FoodDeliveryDbContext> options) : base(options)
        {
        }

        public DbSet<TestEntity> TestEntities { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TestEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired();
            });
        }
    }
}
