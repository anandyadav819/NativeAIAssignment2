using Common.Contracts.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tracking.Domain.Entities;
using Tracking.Domain.ValueObjects;
using Tracking.Infrastructure.Persistence;
using Tracking.Infrastructure.Repositories;

namespace Tracking.UnitTests.Infrastructure;

public class DriverRepositoryTests : IDisposable
{
    private readonly TrackingDbContext _context;
    private readonly DriverRepository _repository;

    public DriverRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<TrackingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TrackingDbContext(options);
        _repository = new DriverRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnDriver_WhenExists()
    {
        // Arrange
        var driver = CreateTestDriver();
        await _context.Drivers.AddAsync(driver);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetByIdAsync(driver.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(driver.Id);
        result.Name.Should().Be(driver.Name);
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
    public async Task GetOnlineDriversAsync_ShouldReturnAvailableAndBusyDrivers()
    {
        // Arrange
        var driver1 = CreateTestDriver("Driver 1");
        driver1.GoOnline();
        
        var driver2 = CreateTestDriver("Driver 2");
        driver2.GoOnline();
        driver2.AssignOrder(Guid.NewGuid());
        
        var driver3 = CreateTestDriver("Driver 3");
        // stays offline

        await _context.Drivers.AddRangeAsync(driver1, driver2, driver3);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetOnlineDriversAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(d => d.Status == DriverStatus.Available);
        result.Should().Contain(d => d.Status == DriverStatus.Busy);
        result.Should().NotContain(d => d.Status == DriverStatus.Offline);
    }

    [Fact]
    public async Task GetOnlineDriversAsync_ShouldReturnEmpty_WhenNoOnlineDrivers()
    {
        // Arrange
        var driver = CreateTestDriver();
        // stays offline

        await _context.Drivers.AddAsync(driver);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetOnlineDriversAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAvailableDriversAsync_ShouldReturnOnlyAvailableDrivers()
    {
        // Arrange
        var driver1 = CreateTestDriver("Driver 1");
        driver1.GoOnline();
        
        var driver2 = CreateTestDriver("Driver 2");
        driver2.GoOnline();
        driver2.AssignOrder(Guid.NewGuid());
        
        var driver3 = CreateTestDriver("Driver 3");
        driver3.GoOnline();

        await _context.Drivers.AddRangeAsync(driver1, driver2, driver3);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetAvailableDriversAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(d => d.Status.Should().Be(DriverStatus.Available));
    }

    [Fact]
    public async Task GetAvailableDriversAsync_ShouldReturnEmpty_WhenNoAvailableDrivers()
    {
        // Arrange
        var driver = CreateTestDriver();
        driver.GoOnline();
        driver.AssignOrder(Guid.NewGuid());

        await _context.Drivers.AddAsync(driver);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetAvailableDriversAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByOrderIdAsync_ShouldReturnDriver_WhenAssignedToOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var driver = CreateTestDriver();
        driver.GoOnline();
        driver.AssignOrder(orderId);

        await _context.Drivers.AddAsync(driver);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetByOrderIdAsync(orderId);

        // Assert
        result.Should().NotBeNull();
        result!.CurrentOrderId.Should().Be(orderId);
    }

    [Fact]
    public async Task GetByOrderIdAsync_ShouldReturnNull_WhenNoDriverAssigned()
    {
        // Arrange
        var nonExistentOrderId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByOrderIdAsync(nonExistentOrderId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByOrderIdAsync_ShouldReturnNull_WhenOrderCompleted()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var driver = CreateTestDriver();
        driver.GoOnline();
        driver.AssignOrder(orderId);
        driver.CompleteDelivery();

        await _context.Drivers.AddAsync(driver);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetByOrderIdAsync(orderId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_ShouldPersistDriver()
    {
        // Arrange
        var driver = CreateTestDriver();

        // Act
        await _repository.AddAsync(driver);
        _context.ChangeTracker.Clear();

        // Assert
        var savedDriver = await _context.Drivers.FindAsync(driver.Id);
        savedDriver.Should().NotBeNull();
        savedDriver!.Name.Should().Be(driver.Name);
        savedDriver.Status.Should().Be(DriverStatus.Offline);
    }

    [Fact]
    public async Task AddAsync_ShouldPersistDriverWithInitialStatus()
    {
        // Arrange
        var driver = CreateTestDriver();
        driver.GoOnline();

        // Act
        await _repository.AddAsync(driver);
        _context.ChangeTracker.Clear();

        // Assert
        var savedDriver = await _context.Drivers.FindAsync(driver.Id);
        savedDriver.Should().NotBeNull();
        savedDriver!.Status.Should().Be(DriverStatus.Available);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateDriverStatus()
    {
        // Arrange
        var driver = CreateTestDriver();
        await _context.Drivers.AddAsync(driver);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var driverToUpdate = await _context.Drivers.FindAsync(driver.Id);
        driverToUpdate!.GoOnline();

        // Act
        await _repository.UpdateAsync(driverToUpdate);
        _context.ChangeTracker.Clear();

        // Assert
        var updatedDriver = await _context.Drivers.FindAsync(driver.Id);
        updatedDriver.Should().NotBeNull();
        updatedDriver!.Status.Should().Be(DriverStatus.Available);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateDriverLocation()
    {
        // Arrange
        var driver = CreateTestDriver();
        await _context.Drivers.AddAsync(driver);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var driverToUpdate = await _context.Drivers.FindAsync(driver.Id);
        driverToUpdate!.UpdateLocation(40.7589, -73.9851);

        // Act
        await _repository.UpdateAsync(driverToUpdate);
        _context.ChangeTracker.Clear();

        // Assert
        var updatedDriver = await _context.Drivers.FindAsync(driver.Id);
        updatedDriver.Should().NotBeNull();
        updatedDriver!.CurrentLocation.Should().NotBeNull();
        updatedDriver.CurrentLocation!.Latitude.Should().Be(40.7589);
        updatedDriver.CurrentLocation.Longitude.Should().Be(-73.9851);
    }

    [Fact]
    public async Task UpdateAsync_ShouldHandleOrderAssignment()
    {
        // Arrange
        var driver = CreateTestDriver();
        driver.GoOnline();
        await _context.Drivers.AddAsync(driver);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var orderId = Guid.NewGuid();
        var driverToUpdate = await _context.Drivers.FindAsync(driver.Id);
        driverToUpdate!.AssignOrder(orderId);

        // Act
        await _repository.UpdateAsync(driverToUpdate);
        _context.ChangeTracker.Clear();

        // Assert
        var updatedDriver = await _context.Drivers.FindAsync(driver.Id);
        updatedDriver.Should().NotBeNull();
        updatedDriver!.CurrentOrderId.Should().Be(orderId);
        updatedDriver.Status.Should().Be(DriverStatus.Busy);
    }

    [Fact]
    public async Task UpdateAsync_ShouldHandleDeliveryCompletion()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var driver = CreateTestDriver();
        driver.GoOnline();
        driver.AssignOrder(orderId);
        await _context.Drivers.AddAsync(driver);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var driverToUpdate = await _context.Drivers.FindAsync(driver.Id);
        driverToUpdate!.CompleteDelivery();

        // Act
        await _repository.UpdateAsync(driverToUpdate);
        _context.ChangeTracker.Clear();

        // Assert
        var updatedDriver = await _context.Drivers.FindAsync(driver.Id);
        updatedDriver.Should().NotBeNull();
        updatedDriver!.CurrentOrderId.Should().BeNull();
        updatedDriver.Status.Should().Be(DriverStatus.Available);
    }

    [Fact]
    public async Task UpdateAsync_ShouldHandleGoingOffline()
    {
        // Arrange
        var driver = CreateTestDriver();
        driver.GoOnline();
        await _context.Drivers.AddAsync(driver);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var driverToUpdate = await _context.Drivers.FindAsync(driver.Id);
        driverToUpdate!.GoOffline();

        // Act
        await _repository.UpdateAsync(driverToUpdate);
        _context.ChangeTracker.Clear();

        // Assert
        var updatedDriver = await _context.Drivers.FindAsync(driver.Id);
        updatedDriver.Should().NotBeNull();
        updatedDriver!.Status.Should().Be(DriverStatus.Offline);
    }

    [Fact]
    public async Task UpdateAsync_ShouldHandleMultipleLocationUpdates()
    {
        // Arrange
        var driver = CreateTestDriver();
        driver.GoOnline();
        await _context.Drivers.AddAsync(driver);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var driverToUpdate = await _context.Drivers.FindAsync(driver.Id);
        driverToUpdate!.UpdateLocation(40.7589, -73.9851);
        driverToUpdate.UpdateLocation(40.7600, -73.9860);
        driverToUpdate.UpdateLocation(40.7614, -73.9776);

        // Act
        await _repository.UpdateAsync(driverToUpdate);
        _context.ChangeTracker.Clear();

        // Assert
        var updatedDriver = await _context.Drivers.FindAsync(driver.Id);
        updatedDriver.Should().NotBeNull();
        updatedDriver!.CurrentLocation.Should().NotBeNull();
        updatedDriver.CurrentLocation!.Latitude.Should().Be(40.7614);
        updatedDriver.CurrentLocation.Longitude.Should().Be(-73.9776);
    }

    private Driver CreateTestDriver(string name = "Test Driver")
    {
        return new Driver(
            name,
            "555-1234",
            "ABC-123");
    }
}
