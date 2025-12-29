using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tracking.Domain.Entities;
using Tracking.Domain.ValueObjects;
using Tracking.Infrastructure.Persistence;
using Tracking.Infrastructure.Repositories;

namespace Tracking.UnitTests.Infrastructure;

public class DeliveryTrackingRepositoryTests : IDisposable
{
    private readonly TrackingDbContext _context;
    private readonly DeliveryTrackingRepository _repository;

    public DeliveryTrackingRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<TrackingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TrackingDbContext(options);
        _repository = new DeliveryTrackingRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnDeliveryTracking_WhenExists()
    {
        // Arrange
        var tracking = CreateTestDeliveryTracking();
        await _context.DeliveryTrackings.AddAsync(tracking);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetByIdAsync(tracking.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(tracking.Id);
        result.OrderId.Should().Be(tracking.OrderId);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldIncludeLocationHistory()
    {
        // Arrange
        var tracking = CreateTestDeliveryTracking();
        await _context.DeliveryTrackings.AddAsync(tracking);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
        
        var trackingToUpdate = await _context.DeliveryTrackings.FindAsync(tracking.Id);
        trackingToUpdate!.UpdateCurrentLocation(new Location(40.7589, -73.9851));
        trackingToUpdate.UpdateCurrentLocation(new Location(40.7600, -73.9860));
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetByIdAsync(tracking.Id);

        // Assert
        result.Should().NotBeNull();
        result!.LocationHistory.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByOrderIdAsync_ShouldReturnDeliveryTracking_WhenExists()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var tracking = CreateTestDeliveryTracking(orderId: orderId);
        await _context.DeliveryTrackings.AddAsync(tracking);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetByOrderIdAsync(orderId);

        // Assert
        result.Should().NotBeNull();
        result!.OrderId.Should().Be(orderId);
    }

    [Fact]
    public async Task GetByOrderIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        var nonExistentOrderId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByOrderIdAsync(nonExistentOrderId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByOrderIdAsync_ShouldIncludeLocationHistory()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var tracking = CreateTestDeliveryTracking(orderId: orderId);
        await _context.DeliveryTrackings.AddAsync(tracking);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
        
        var trackingToUpdate = await _context.DeliveryTrackings.FindAsync(tracking.Id);
        trackingToUpdate!.UpdateCurrentLocation(new Location(40.7589, -73.9851));
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetByOrderIdAsync(orderId);

        // Assert
        result.Should().NotBeNull();
        result!.LocationHistory.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetActiveDeliveriesAsync_ShouldReturnOnlyActiveDeliveries()
    {
        // Arrange
        var tracking1 = CreateTestDeliveryTracking();
        tracking1.MarkPickedUp();
        
        var tracking2 = CreateTestDeliveryTracking();
        tracking2.MarkPickedUp();
        tracking2.MarkDelivered();
        
        var tracking3 = CreateTestDeliveryTracking();
        tracking3.MarkPickedUp();

        await _context.DeliveryTrackings.AddRangeAsync(tracking1, tracking2, tracking3);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetActiveDeliveriesAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(d => d.DeliveredAt.Should().BeNull());
    }

    [Fact]
    public async Task GetActiveDeliveriesAsync_ShouldReturnEmpty_WhenNoActiveDeliveries()
    {
        // Arrange
        var tracking = CreateTestDeliveryTracking();
        tracking.MarkPickedUp();
        tracking.MarkDelivered();

        await _context.DeliveryTrackings.AddAsync(tracking);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetActiveDeliveriesAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetActiveDeliveriesAsync_ShouldIncludeLocationHistory()
    {
        // Arrange
        var tracking = CreateTestDeliveryTracking();
        await _context.DeliveryTrackings.AddAsync(tracking);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
        
        var trackingToUpdate = await _context.DeliveryTrackings.FindAsync(tracking.Id);
        trackingToUpdate!.UpdateCurrentLocation(new Location(40.7589, -73.9851));
        trackingToUpdate.MarkPickedUp();
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetActiveDeliveriesAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().LocationHistory.Should().HaveCount(1);
    }

    [Fact]
    public async Task AddAsync_ShouldPersistDeliveryTracking()
    {
        // Arrange
        var tracking = CreateTestDeliveryTracking();

        // Act
        await _repository.AddAsync(tracking);
        _context.ChangeTracker.Clear();

        // Assert
        var savedTracking = await _context.DeliveryTrackings.FindAsync(tracking.Id);
        savedTracking.Should().NotBeNull();
        savedTracking!.OrderId.Should().Be(tracking.OrderId);
    }

    [Fact]
    public async Task AddAsync_ShouldSaveLocationHistory()
    {
        // Arrange
        var tracking = CreateTestDeliveryTracking();

        // Act - save first
        await _repository.AddAsync(tracking);
        _context.ChangeTracker.Clear();
        
        // Update with locations
        var trackingToUpdate = await _context.DeliveryTrackings.FindAsync(tracking.Id);
        trackingToUpdate!.UpdateCurrentLocation(new Location(40.7589, -73.9851));
        trackingToUpdate.UpdateCurrentLocation(new Location(40.7600, -73.9860));
        await _repository.UpdateAsync(trackingToUpdate);
        _context.ChangeTracker.Clear();

        // Assert
        var savedTracking = await _context.DeliveryTrackings
            .Include(dt => dt.LocationHistory)
            .FirstOrDefaultAsync(dt => dt.Id == tracking.Id);
        
        savedTracking.Should().NotBeNull();
        savedTracking!.LocationHistory.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateDeliveryTracking()
    {
        // Arrange
        var tracking = CreateTestDeliveryTracking();
        await _context.DeliveryTrackings.AddAsync(tracking);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var trackingToUpdate = await _context.DeliveryTrackings.FindAsync(tracking.Id);
        trackingToUpdate!.UpdateCurrentLocation(new Location(40.7589, -73.9851));

        // Act
        await _repository.UpdateAsync(trackingToUpdate);
        _context.ChangeTracker.Clear();

        // Assert
        var updatedTracking = await _context.DeliveryTrackings
            .Include(dt => dt.LocationHistory)
            .FirstOrDefaultAsync(dt => dt.Id == tracking.Id);
        
        updatedTracking.Should().NotBeNull();
        updatedTracking!.CurrentLocation.Should().NotBeNull();
        updatedTracking.LocationHistory.Should().HaveCount(1);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateDeliveryStatus()
    {
        // Arrange
        var tracking = CreateTestDeliveryTracking();
        await _context.DeliveryTrackings.AddAsync(tracking);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var trackingToUpdate = await _context.DeliveryTrackings.FindAsync(tracking.Id);
        trackingToUpdate!.MarkPickedUp();

        // Act
        await _repository.UpdateAsync(trackingToUpdate);
        _context.ChangeTracker.Clear();

        // Assert
        var updatedTracking = await _context.DeliveryTrackings.FindAsync(tracking.Id);
        updatedTracking.Should().NotBeNull();
        updatedTracking!.PickedUpAt.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_ShouldHandleFullDeliveryLifecycle()
    {
        // Arrange
        var tracking = CreateTestDeliveryTracking();
        await _context.DeliveryTrackings.AddAsync(tracking);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var trackingToUpdate = await _context.DeliveryTrackings.FindAsync(tracking.Id);
        trackingToUpdate!.UpdateCurrentLocation(new Location(40.7589, -73.9851));
        trackingToUpdate.MarkPickedUp();
        trackingToUpdate.UpdateCurrentLocation(new Location(40.7600, -73.9860));
        trackingToUpdate.MarkDelivered();

        // Act
        await _repository.UpdateAsync(trackingToUpdate);
        _context.ChangeTracker.Clear();

        // Assert
        var updatedTracking = await _context.DeliveryTrackings
            .Include(dt => dt.LocationHistory)
            .FirstOrDefaultAsync(dt => dt.Id == tracking.Id);
        
        updatedTracking.Should().NotBeNull();
        updatedTracking!.PickedUpAt.Should().NotBeNull();
        updatedTracking.DeliveredAt.Should().NotBeNull();
        updatedTracking.LocationHistory.Should().HaveCount(2);
    }

    private DeliveryTracking CreateTestDeliveryTracking(Guid? orderId = null, Guid? driverId = null)
    {
        var pickupLocation = new Location(40.7580, -73.9855);
        var deliveryLocation = new Location(40.7614, -73.9776);
        
        return new DeliveryTracking(
            orderId ?? Guid.NewGuid(),
            driverId ?? Guid.NewGuid(),
            pickupLocation,
            deliveryLocation);
    }
}
