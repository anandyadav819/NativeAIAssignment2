using FluentAssertions;
using Menu.Domain.Entities;
using Menu.Infrastructure.Persistence;
using Menu.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Menu.UnitTests.Infrastructure;

public class MenuItemRepositoryTests : IDisposable
{
    private readonly MenuDbContext _context;
    private readonly MenuItemRepository _repository;

    public MenuItemRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<MenuDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new MenuDbContext(options);
        _repository = new MenuItemRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetByIdAsync_WhenMenuItemExists_ShouldReturnMenuItem()
    {
        // Arrange
        var restaurant = CreateTestRestaurant();
        restaurant.AddMenuItem("Test Item", "Description", 10.00m, "Main Course");
        await _context.Restaurants.AddAsync(restaurant);
        await _context.SaveChangesAsync();

        var menuItemId = restaurant.MenuItems.First().Id;

        _context.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetByIdAsync(menuItemId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(menuItemId);
        result.Name.Should().Be("Test Item");
    }

    [Fact]
    public async Task GetByIdAsync_WhenMenuItemDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldFindMenuItemInAnyRestaurant()
    {
        // Arrange
        var restaurant1 = CreateTestRestaurant("Restaurant 1");
        restaurant1.AddMenuItem("Item 1", "Description 1", 10.00m, "Main Course");
        
        var restaurant2 = CreateTestRestaurant("Restaurant 2");
        restaurant2.AddMenuItem("Item 2", "Description 2", 15.00m, "Dessert");

        await _context.Restaurants.AddRangeAsync(restaurant1, restaurant2);
        await _context.SaveChangesAsync();

        var menuItemId = restaurant2.MenuItems.First().Id;
        _context.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetByIdAsync(menuItemId);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Item 2");
        result.RestaurantId.Should().Be(restaurant2.Id);
    }

    [Fact]
    public async Task GetByRestaurantIdAsync_ShouldReturnAllMenuItemsForRestaurant()
    {
        // Arrange
        var restaurant = CreateTestRestaurant();
        restaurant.AddMenuItem("Item 1", "Description 1", 10.00m, "Main Course");
        restaurant.AddMenuItem("Item 2", "Description 2", 15.00m, "Dessert");
        restaurant.AddMenuItem("Item 3", "Description 3", 12.00m, "Appetizer");

        await _context.Restaurants.AddAsync(restaurant);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetByRestaurantIdAsync(restaurant.Id);

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(m => m.Name == "Item 1");
        result.Should().Contain(m => m.Name == "Item 2");
        result.Should().Contain(m => m.Name == "Item 3");
    }

    [Fact]
    public async Task GetByRestaurantIdAsync_WhenRestaurantHasNoItems_ShouldReturnEmptyList()
    {
        // Arrange
        var restaurant = CreateTestRestaurant();
        await _context.Restaurants.AddAsync(restaurant);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByRestaurantIdAsync(restaurant.Id);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByRestaurantIdAsync_WhenRestaurantDoesNotExist_ShouldReturnEmptyList()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByRestaurantIdAsync(nonExistentId);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByRestaurantIdAsync_ShouldOnlyReturnItemsForSpecifiedRestaurant()
    {
        // Arrange
        var restaurant1 = CreateTestRestaurant("Restaurant 1");
        restaurant1.AddMenuItem("Restaurant 1 Item", "Description", 10.00m, "Main Course");
        
        var restaurant2 = CreateTestRestaurant("Restaurant 2");
        restaurant2.AddMenuItem("Restaurant 2 Item", "Description", 15.00m, "Dessert");

        await _context.Restaurants.AddRangeAsync(restaurant1, restaurant2);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetByRestaurantIdAsync(restaurant1.Id);

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Restaurant 1 Item");
        result[0].RestaurantId.Should().Be(restaurant1.Id);
    }

    [Fact]
    public async Task GetByCategoryAsync_ShouldReturnItemsInCategory()
    {
        // Arrange
        var restaurant = CreateTestRestaurant();
        restaurant.AddMenuItem("Main 1", "Description 1", 10.00m, "Main Course");
        restaurant.AddMenuItem("Main 2", "Description 2", 15.00m, "Main Course");
        restaurant.AddMenuItem("Dessert 1", "Description 3", 8.00m, "Dessert");

        await _context.Restaurants.AddAsync(restaurant);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetByCategoryAsync(restaurant.Id, "Main Course");

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(m => m.Category.Should().Be("Main Course"));
        result.Should().Contain(m => m.Name == "Main 1");
        result.Should().Contain(m => m.Name == "Main 2");
    }

    [Fact]
    public async Task GetByCategoryAsync_WhenNoCategoryMatch_ShouldReturnEmptyList()
    {
        // Arrange
        var restaurant = CreateTestRestaurant();
        restaurant.AddMenuItem("Main 1", "Description 1", 10.00m, "Main Course");

        await _context.Restaurants.AddAsync(restaurant);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByCategoryAsync(restaurant.Id, "Appetizer");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByCategoryAsync_WhenRestaurantDoesNotExist_ShouldReturnEmptyList()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByCategoryAsync(nonExistentId, "Main Course");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByCategoryAsync_ShouldOnlyReturnItemsForSpecifiedRestaurant()
    {
        // Arrange
        var restaurant1 = CreateTestRestaurant("Restaurant 1");
        restaurant1.AddMenuItem("Restaurant 1 Main", "Description", 10.00m, "Main Course");
        
        var restaurant2 = CreateTestRestaurant("Restaurant 2");
        restaurant2.AddMenuItem("Restaurant 2 Main", "Description", 15.00m, "Main Course");

        await _context.Restaurants.AddRangeAsync(restaurant1, restaurant2);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetByCategoryAsync(restaurant1.Id, "Main Course");

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Restaurant 1 Main");
    }

    [Fact]
    public async Task SearchAsync_ShouldFindByName()
    {
        // Arrange
        var restaurant = CreateTestRestaurant();
        restaurant.AddMenuItem("Pizza Margherita", "Classic pizza", 12.00m, "Main Course");
        restaurant.AddMenuItem("Pizza Pepperoni", "Pepperoni pizza", 14.00m, "Main Course");
        restaurant.AddMenuItem("Pasta Carbonara", "Creamy pasta", 13.00m, "Main Course");

        await _context.Restaurants.AddAsync(restaurant);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        // Act
        var result = await _repository.SearchAsync("Pizza", 1, 10);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(m => m.Name == "Pizza Margherita");
        result.Should().Contain(m => m.Name == "Pizza Pepperoni");
        result.Should().NotContain(m => m.Name == "Pasta Carbonara");
    }

    [Fact]
    public async Task SearchAsync_ShouldFindByDescription()
    {
        // Arrange
        var restaurant = CreateTestRestaurant();
        restaurant.AddMenuItem("Item 1", "Spicy chicken dish", 12.00m, "Main Course");
        restaurant.AddMenuItem("Item 2", "Spicy vegetarian curry", 10.00m, "Main Course");
        restaurant.AddMenuItem("Item 3", "Sweet dessert", 8.00m, "Dessert");

        await _context.Restaurants.AddAsync(restaurant);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        // Act
        var result = await _repository.SearchAsync("Spicy", 1, 10);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(m => m.Name == "Item 1");
        result.Should().Contain(m => m.Name == "Item 2");
    }

    [Fact]
    public async Task SearchAsync_ShouldSearchAcrossAllRestaurants()
    {
        // Arrange
        var restaurant1 = CreateTestRestaurant("Restaurant 1");
        restaurant1.AddMenuItem("Pizza Margherita", "Classic pizza", 12.00m, "Main Course");
        
        var restaurant2 = CreateTestRestaurant("Restaurant 2");
        restaurant2.AddMenuItem("Pizza Pepperoni", "Pepperoni pizza", 14.00m, "Main Course");

        await _context.Restaurants.AddRangeAsync(restaurant1, restaurant2);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        // Act
        var result = await _repository.SearchAsync("Pizza", 1, 10);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(m => m.RestaurantId == restaurant1.Id);
        result.Should().Contain(m => m.RestaurantId == restaurant2.Id);
    }

    [Fact]
    public async Task SearchAsync_ShouldRespectPagination()
    {
        // Arrange
        var restaurant = CreateTestRestaurant();
        for (int i = 1; i <= 8; i++)
        {
            restaurant.AddMenuItem($"Pizza {i}", $"Description {i}", 10.00m + i, "Main Course");
        }

        await _context.Restaurants.AddAsync(restaurant);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        // Act
        var page1 = await _repository.SearchAsync("Pizza", 1, 3);
        var page2 = await _repository.SearchAsync("Pizza", 2, 3);
        var page3 = await _repository.SearchAsync("Pizza", 3, 3);

        // Assert
        page1.Should().HaveCount(3);
        page2.Should().HaveCount(3);
        page3.Should().HaveCount(2); // Last page has remaining items

        var allIds = page1.Select(m => m.Id)
            .Concat(page2.Select(m => m.Id))
            .Concat(page3.Select(m => m.Id));
        allIds.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task SearchAsync_ShouldOrderByName()
    {
        // Arrange
        var restaurant = CreateTestRestaurant();
        restaurant.AddMenuItem("Pizza Pepperoni", "Description", 14.00m, "Main Course");
        restaurant.AddMenuItem("Pizza Margherita", "Description", 12.00m, "Main Course");
        restaurant.AddMenuItem("Pizza Hawaiian", "Description", 13.00m, "Main Course");

        await _context.Restaurants.AddAsync(restaurant);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        // Act
        var result = await _repository.SearchAsync("Pizza", 1, 10);

        // Assert
        result.Should().HaveCount(3);
        result[0].Name.Should().Be("Pizza Hawaiian");
        result[1].Name.Should().Be("Pizza Margherita");
        result[2].Name.Should().Be("Pizza Pepperoni");
    }

    [Fact]
    public async Task SearchAsync_WhenNoMatch_ShouldReturnEmptyList()
    {
        // Arrange
        var restaurant = CreateTestRestaurant();
        restaurant.AddMenuItem("Pasta Carbonara", "Creamy pasta", 13.00m, "Main Course");

        await _context.Restaurants.AddAsync(restaurant);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync("Pizza", 1, 10);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAsync_WhenNoMenuItems_ShouldReturnEmptyList()
    {
        // Act
        var result = await _repository.SearchAsync("Pizza", 1, 10);

        // Assert
        result.Should().BeEmpty();
    }

    private Restaurant CreateTestRestaurant(string name = "Test Restaurant")
    {
        return new Restaurant(
            name,
            "Test Description",
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
