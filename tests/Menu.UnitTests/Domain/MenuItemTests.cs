using FluentAssertions;
using MenuItemEntity = Menu.Domain.Entities.MenuItem;

namespace Menu.UnitTests.Domain;

public class MenuItemTests
{
    private readonly Guid _restaurantId = Guid.NewGuid();

    [Fact]
    public void Constructor_Should_CreateMenuItemWithCorrectProperties()
    {
        // Act
        var menuItem = new MenuItemEntity(
            _restaurantId, "Margherita Pizza", "Classic Italian pizza", 12.99m, "Pizza", "pizza.jpg");

        // Assert
        menuItem.Id.Should().NotBeEmpty();
        menuItem.RestaurantId.Should().Be(_restaurantId);
        menuItem.Name.Should().Be("Margherita Pizza");
        menuItem.Description.Should().Be("Classic Italian pizza");
        menuItem.Price.Should().Be(12.99m);
        menuItem.Category.Should().Be("Pizza");
        menuItem.ImageUrl.Should().Be("pizza.jpg");
        menuItem.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public void Constructor_WithoutImageUrl_Should_CreateMenuItemWithNullImage()
    {
        // Act
        var menuItem = new MenuItemEntity(
            _restaurantId, "Caesar Salad", "Fresh salad", 8.99m, "Salad");

        // Assert
        menuItem.ImageUrl.Should().BeNull();
        menuItem.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public void Update_Should_UpdateAllProperties()
    {
        // Arrange
        var menuItem = new MenuItemEntity(
            _restaurantId, "Margherita Pizza", "Classic pizza", 12.99m, "Pizza", "old-image.jpg");

        // Act
        menuItem.Update("Margherita Pizza Deluxe", "Updated description", 14.99m, "Pizza", "new-image.jpg");

        // Assert
        menuItem.Name.Should().Be("Margherita Pizza Deluxe");
        menuItem.Description.Should().Be("Updated description");
        menuItem.Price.Should().Be(14.99m);
        menuItem.Category.Should().Be("Pizza");
        menuItem.ImageUrl.Should().Be("new-image.jpg");
    }

    [Fact]
    public void Update_WithoutImageUrl_Should_UpdateWithNullImage()
    {
        // Arrange
        var menuItem = new MenuItemEntity(
            _restaurantId, "Margherita Pizza", "Classic pizza", 12.99m, "Pizza", "old-image.jpg");

        // Act
        menuItem.Update("Margherita Pizza", "Updated description", 14.99m, "Pizza");

        // Assert
        menuItem.ImageUrl.Should().BeNull();
    }

    [Fact]
    public void SetAvailability_ToFalse_Should_MarkItemAsUnavailable()
    {
        // Arrange
        var menuItem = new MenuItemEntity(
            _restaurantId, "Margherita Pizza", "Classic pizza", 12.99m, "Pizza");

        // Act
        menuItem.SetAvailability(false);

        // Assert
        menuItem.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public void SetAvailability_ToTrue_Should_MarkItemAsAvailable()
    {
        // Arrange
        var menuItem = new MenuItemEntity(
            _restaurantId, "Margherita Pizza", "Classic pizza", 12.99m, "Pizza");
        menuItem.SetAvailability(false);

        // Act
        menuItem.SetAvailability(true);

        // Assert
        menuItem.IsAvailable.Should().BeTrue();
    }

    [Theory]
    [InlineData(0.99)]
    [InlineData(9.99)]
    [InlineData(99.99)]
    [InlineData(999.99)]
    public void Constructor_WithDifferentPrices_Should_CreateMenuItemCorrectly(decimal price)
    {
        // Act
        var menuItem = new MenuItemEntity(
            _restaurantId, "Test Item", "Test description", price, "Test Category");

        // Assert
        menuItem.Price.Should().Be(price);
    }

    [Theory]
    [InlineData("Pizza")]
    [InlineData("Pasta")]
    [InlineData("Salad")]
    [InlineData("Dessert")]
    [InlineData("Beverage")]
    public void Constructor_WithDifferentCategories_Should_CreateMenuItemCorrectly(string category)
    {
        // Act
        var menuItem = new MenuItemEntity(
            _restaurantId, "Test Item", "Test description", 10.99m, category);

        // Assert
        menuItem.Category.Should().Be(category);
    }
}
