using Common.Contracts.Enums;
using FluentAssertions;
using Menu.Domain.Entities;
using Menu.Infrastructure.Persistence;
using Menu.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Menu.UnitTests.Infrastructure;

public class RestaurantRepositoryTests : IDisposable
{
    private readonly MenuDbContext _context;
    private readonly RestaurantRepository _repository;

    public RestaurantRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<MenuDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new MenuDbContext(options);
        _repository = new RestaurantRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetByIdAsync_WhenRestaurantExists_ShouldReturnRestaurant()
    {
        // Arrange
        var restaurant = CreateTestRestaurant("Test Restaurant");
        await _context.Restaurants.AddAsync(restaurant);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(restaurant.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(restaurant.Id);
        result.Name.Should().Be("Test Restaurant");
    }

    [Fact]
    public async Task GetByIdAsync_WhenRestaurantDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldIncludeMenuItems()
    {
        // Arrange
        var restaurant = CreateTestRestaurant("Test Restaurant");
        restaurant.AddMenuItem("Item 1", "Description 1", 10.00m, "Main Course");
        restaurant.AddMenuItem("Item 2", "Description 2", 15.00m, "Dessert");
        await _context.Restaurants.AddAsync(restaurant);
        await _context.SaveChangesAsync();

        // Clear the context to ensure fresh load
        _context.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetByIdAsync(restaurant.Id);

        // Assert
        result.Should().NotBeNull();
        result!.MenuItems.Should().HaveCount(2);
        result.MenuItems.Should().Contain(m => m.Name == "Item 1");
        result.MenuItems.Should().Contain(m => m.Name == "Item 2");
    }

    [Fact]
    public async Task GetActiveRestaurantsAsync_ShouldReturnOnlyOpenRestaurants()
    {
        // Arrange
        var restaurant1 = CreateTestRestaurant("Open Restaurant 1");
        restaurant1.Activate();
        
        var restaurant2 = CreateTestRestaurant("Closed Restaurant");
        // restaurant2 stays closed (default status)
        
        var restaurant3 = CreateTestRestaurant("Open Restaurant 2");
        restaurant3.Activate();

        await _context.Restaurants.AddRangeAsync(restaurant1, restaurant2, restaurant3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveRestaurantsAsync(1, 10);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(r => r.Status.Should().Be(RestaurantStatus.Open));
        result.Should().Contain(r => r.Name == "Open Restaurant 1");
        result.Should().Contain(r => r.Name == "Open Restaurant 2");
        result.Should().NotContain(r => r.Name == "Closed Restaurant");
    }

    [Fact]
    public async Task GetActiveRestaurantsAsync_ShouldRespectPagination()
    {
        // Arrange
        for (int i = 1; i <= 15; i++)
        {
            var restaurant = CreateTestRestaurant($"Restaurant {i:D2}");
            restaurant.Activate();
            await _context.Restaurants.AddAsync(restaurant);
        }
        await _context.SaveChangesAsync();

        // Act
        var page1 = await _repository.GetActiveRestaurantsAsync(1, 5);
        var page2 = await _repository.GetActiveRestaurantsAsync(2, 5);
        var page3 = await _repository.GetActiveRestaurantsAsync(3, 5);

        // Assert
        page1.Should().HaveCount(5);
        page2.Should().HaveCount(5);
        page3.Should().HaveCount(5);

        // Verify no overlap
        var allIds = page1.Select(r => r.Id)
            .Concat(page2.Select(r => r.Id))
            .Concat(page3.Select(r => r.Id));
        allIds.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task GetActiveRestaurantsAsync_ShouldOrderByName()
    {
        // Arrange
        var restaurantC = CreateTestRestaurant("Charlie's Restaurant");
        restaurantC.Activate();
        var restaurantA = CreateTestRestaurant("Alice's Restaurant");
        restaurantA.Activate();
        var restaurantB = CreateTestRestaurant("Bob's Restaurant");
        restaurantB.Activate();

        await _context.Restaurants.AddRangeAsync(restaurantC, restaurantA, restaurantB);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveRestaurantsAsync(1, 10);

        // Assert
        result.Should().HaveCount(3);
        result[0].Name.Should().Be("Alice's Restaurant");
        result[1].Name.Should().Be("Bob's Restaurant");
        result[2].Name.Should().Be("Charlie's Restaurant");
    }

    [Fact]
    public async Task GetActiveRestaurantsAsync_ShouldIncludeMenuItems()
    {
        // Arrange
        var restaurant = CreateTestRestaurant("Test Restaurant");
        restaurant.Activate();
        restaurant.AddMenuItem("Item 1", "Description 1", 10.00m, "Main Course");
        await _context.Restaurants.AddAsync(restaurant);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetActiveRestaurantsAsync(1, 10);

        // Assert
        result.Should().HaveCount(1);
        result[0].MenuItems.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetActiveRestaurantCountAsync_ShouldReturnCorrectCount()
    {
        // Arrange
        var restaurant1 = CreateTestRestaurant("Open Restaurant 1");
        restaurant1.Activate();
        
        var restaurant2 = CreateTestRestaurant("Closed Restaurant");
        // restaurant2 stays closed (default status)
        
        var restaurant3 = CreateTestRestaurant("Open Restaurant 2");
        restaurant3.Activate();

        await _context.Restaurants.AddRangeAsync(restaurant1, restaurant2, restaurant3);
        await _context.SaveChangesAsync();

        // Act
        var count = await _repository.GetActiveRestaurantCountAsync();

        // Assert
        count.Should().Be(2);
    }

    [Fact]
    public async Task GetActiveRestaurantCountAsync_WhenNoRestaurants_ShouldReturnZero()
    {
        // Act
        var count = await _repository.GetActiveRestaurantCountAsync();

        // Assert
        count.Should().Be(0);
    }

    [Fact]
    public async Task SearchRestaurantsAsync_ShouldFindByName()
    {
        // Arrange
        var restaurant1 = CreateTestRestaurant("Pizza Paradise");
        restaurant1.Activate();
        var restaurant2 = CreateTestRestaurant("Burger King");
        restaurant2.Activate();
        var restaurant3 = CreateTestRestaurant("Pizza Palace");
        restaurant3.Activate();

        await _context.Restaurants.AddRangeAsync(restaurant1, restaurant2, restaurant3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchRestaurantsAsync("Pizza", 1, 10);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(r => r.Name == "Pizza Paradise");
        result.Should().Contain(r => r.Name == "Pizza Palace");
        result.Should().NotContain(r => r.Name == "Burger King");
    }

    [Fact]
    public async Task SearchRestaurantsAsync_ShouldFindByDescription()
    {
        // Arrange
        var restaurant1 = CreateTestRestaurant("Restaurant 1", "Italian cuisine and pasta");
        restaurant1.Activate();
        var restaurant2 = CreateTestRestaurant("Restaurant 2", "Chinese food");
        restaurant2.Activate();
        var restaurant3 = CreateTestRestaurant("Restaurant 3", "Italian pizza");
        restaurant3.Activate();

        await _context.Restaurants.AddRangeAsync(restaurant1, restaurant2, restaurant3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchRestaurantsAsync("Italian", 1, 10);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(r => r.Name == "Restaurant 1");
        result.Should().Contain(r => r.Name == "Restaurant 3");
    }

    [Fact]
    public async Task SearchRestaurantsAsync_ShouldOnlyReturnOpenRestaurants()
    {
        // Arrange
        var restaurant1 = CreateTestRestaurant("Pizza Place Open");
        restaurant1.Activate();
        var restaurant2 = CreateTestRestaurant("Pizza Place Closed");
        // restaurant2 stays closed (default status)

        await _context.Restaurants.AddRangeAsync(restaurant1, restaurant2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchRestaurantsAsync("Pizza", 1, 10);

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Pizza Place Open");
    }

    [Fact]
    public async Task SearchRestaurantsAsync_ShouldRespectPagination()
    {
        // Arrange
        for (int i = 1; i <= 8; i++)
        {
            var restaurant = CreateTestRestaurant($"Pizza Place {i}");
            restaurant.Activate();
            await _context.Restaurants.AddAsync(restaurant);
        }
        await _context.SaveChangesAsync();

        // Act
        var page1 = await _repository.SearchRestaurantsAsync("Pizza", 1, 3);
        var page2 = await _repository.SearchRestaurantsAsync("Pizza", 2, 3);

        // Assert
        page1.Should().HaveCount(3);
        page2.Should().HaveCount(3);
        
        var allIds = page1.Select(r => r.Id).Concat(page2.Select(r => r.Id));
        allIds.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task SearchRestaurantsAsync_WhenNoMatch_ShouldReturnEmpty()
    {
        // Arrange
        var restaurant = CreateTestRestaurant("Italian Restaurant");
        restaurant.Activate();
        await _context.Restaurants.AddAsync(restaurant);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchRestaurantsAsync("Chinese", 1, 10);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task AddAsync_ShouldAddRestaurantToDatabase()
    {
        // Arrange
        var restaurant = CreateTestRestaurant("New Restaurant");

        // Act
        await _repository.AddAsync(restaurant);

        // Assert
        var savedRestaurant = await _context.Restaurants.FindAsync(restaurant.Id);
        savedRestaurant.Should().NotBeNull();
        savedRestaurant!.Name.Should().Be("New Restaurant");
    }

    [Fact]
    public async Task AddAsync_ShouldSaveMenuItems()
    {
        // Arrange
        var restaurant = CreateTestRestaurant("New Restaurant");
        restaurant.AddMenuItem("Item 1", "Description 1", 10.00m, "Main Course");
        restaurant.AddMenuItem("Item 2", "Description 2", 15.00m, "Dessert");

        // Act
        await _repository.AddAsync(restaurant);

        // Assert
        _context.ChangeTracker.Clear();
        var savedRestaurant = await _context.Restaurants
            .Include(r => r.MenuItems)
            .FirstOrDefaultAsync(r => r.Id == restaurant.Id);
        
        savedRestaurant.Should().NotBeNull();
        savedRestaurant!.MenuItems.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateRestaurantInDatabase()
    {
        // Arrange
        var restaurant = CreateTestRestaurant("Old Name");
        await _context.Restaurants.AddAsync(restaurant);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        // Act
        var restaurantToUpdate = await _context.Restaurants.FindAsync(restaurant.Id);
        restaurantToUpdate!.UpdateInfo("New Name", "New Description", "New Address", "1234567890",
            new TimeSpan(10, 0, 0), new TimeSpan(23, 0, 0), 6.00m, 20.00m, 35);
        await _repository.UpdateAsync(restaurantToUpdate);

        // Assert
        _context.ChangeTracker.Clear();
        var updatedRestaurant = await _context.Restaurants.FindAsync(restaurant.Id);
        updatedRestaurant.Should().NotBeNull();
        updatedRestaurant!.Name.Should().Be("New Name");
        updatedRestaurant.Description.Should().Be("New Description");
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistStatusChanges()
    {
        // Arrange
        var restaurant = CreateTestRestaurant("Test Restaurant");
        await _context.Restaurants.AddAsync(restaurant);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        // Act
        var restaurantToUpdate = await _context.Restaurants.FindAsync(restaurant.Id);
        restaurantToUpdate!.Activate();
        await _repository.UpdateAsync(restaurantToUpdate);

        // Assert
        _context.ChangeTracker.Clear();
        var updatedRestaurant = await _context.Restaurants.FindAsync(restaurant.Id);
        updatedRestaurant.Should().NotBeNull();
        updatedRestaurant!.Status.Should().Be(RestaurantStatus.Open);
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistMenuItemChanges()
    {
        // Arrange
        var restaurant = CreateTestRestaurant("Test Restaurant");
        restaurant.AddMenuItem("Original Item", "Description", 10.00m, "Main Course");
        await _context.Restaurants.AddAsync(restaurant);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        // Act
        var restaurantToUpdate = await _context.Restaurants
            .Include(r => r.MenuItems)
            .FirstAsync(r => r.Id == restaurant.Id);
        restaurantToUpdate.AddMenuItem("New Item", "New Description", 20.00m, "Dessert");
        await _repository.UpdateAsync(restaurantToUpdate);

        // Assert
        _context.ChangeTracker.Clear();
        var updatedRestaurant = await _context.Restaurants
            .Include(r => r.MenuItems)
            .FirstAsync(r => r.Id == restaurant.Id);
        updatedRestaurant.MenuItems.Should().HaveCount(2);
        updatedRestaurant.MenuItems.Should().Contain(m => m.Name == "New Item");
    }

    private Restaurant CreateTestRestaurant(string name, string description = "Test Description")
    {
        return new Restaurant(
            name,
            description,
            "123 Test Street",
            "555-0123",
            new TimeSpan(9, 0, 0),
            new TimeSpan(22, 0, 0),
            5.00m,
            15.00m,
            30
        );
    }
}
