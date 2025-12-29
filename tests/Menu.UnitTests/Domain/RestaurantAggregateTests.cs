using Common.Contracts.Enums;
using FluentAssertions;
using Menu.Domain.Events;
using RestaurantEntity = Menu.Domain.Entities.Restaurant;

namespace Menu.UnitTests.Domain;

public class RestaurantAggregateTests
{
    private const string RestaurantName = "Pizza Palace";
    private const string Description = "Best pizza in town";
    private const string Address = "123 Main St";
    private const string PhoneNumber = "555-1234";
    private readonly TimeSpan _openingTime = new(9, 0, 0);
    private readonly TimeSpan _closingTime = new(22, 0, 0);

    [Fact]
    public void Constructor_Should_CreateRestaurantWithClosedStatus()
    {
        // Act
        var restaurant = new RestaurantEntity(
            RestaurantName, Description, Address, PhoneNumber,
            _openingTime, _closingTime, 5.99m, 15.00m, 30);

        // Assert
        restaurant.Id.Should().NotBeEmpty();
        restaurant.Name.Should().Be(RestaurantName);
        restaurant.Description.Should().Be(Description);
        restaurant.Address.Should().Be(Address);
        restaurant.PhoneNumber.Should().Be(PhoneNumber);
        restaurant.Status.Should().Be(RestaurantStatus.Closed);
        restaurant.OpeningTime.Should().Be(_openingTime);
        restaurant.ClosingTime.Should().Be(_closingTime);
        restaurant.DeliveryFee.Should().Be(5.99m);
        restaurant.MinimumOrderAmount.Should().Be(15.00m);
        restaurant.EstimatedPreparationTimeMinutes.Should().Be(30);
        restaurant.MenuItems.Should().BeEmpty();
        restaurant.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<RestaurantCreatedDomainEvent>();
    }

    [Fact]
    public void Activate_Should_ChangeStatusToOpen()
    {
        // Arrange
        var restaurant = CreateTestRestaurant();

        // Act
        restaurant.Activate();

        // Assert
        restaurant.Status.Should().Be(RestaurantStatus.Open);
        var statusEvent = restaurant.DomainEvents.OfType<RestaurantStatusChangedDomainEvent>().FirstOrDefault();
        statusEvent.Should().NotBeNull();
        statusEvent!.NewStatus.Should().Be(RestaurantStatus.Open);
    }

    [Fact]
    public void Activate_WhenAlreadyOpen_Should_ThrowInvalidOperationException()
    {
        // Arrange
        var restaurant = CreateTestRestaurant();
        restaurant.Activate();

        // Act
        var act = () => restaurant.Activate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Restaurant is already open");
    }

    [Fact]
    public void Deactivate_Should_ChangeStatusToClosed()
    {
        // Arrange
        var restaurant = CreateTestRestaurant();
        restaurant.Activate();

        // Act
        restaurant.Deactivate();

        // Assert
        restaurant.Status.Should().Be(RestaurantStatus.Closed);
    }

    [Fact]
    public void Deactivate_WhenAlreadyClosed_Should_ThrowInvalidOperationException()
    {
        // Arrange
        var restaurant = CreateTestRestaurant();

        // Act
        var act = () => restaurant.Deactivate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Restaurant is already closed");
    }

    [Fact]
    public void Suspend_Should_ChangeStatusToTemporarilyClosed()
    {
        // Arrange
        var restaurant = CreateTestRestaurant();
        restaurant.Activate();

        // Act
        restaurant.Suspend();

        // Assert
        restaurant.Status.Should().Be(RestaurantStatus.TemporarilyClosed);
    }

    [Fact]
    public void AddMenuItem_Should_AddItemToMenuAndRaiseEvent()
    {
        // Arrange
        var restaurant = CreateTestRestaurant();

        // Act
        var menuItem = restaurant.AddMenuItem("Margherita Pizza", "Classic pizza", 12.99m, "Pizza", null);

        // Assert
        menuItem.Should().NotBeNull();
        menuItem.Name.Should().Be("Margherita Pizza");
        menuItem.Price.Should().Be(12.99m);
        restaurant.MenuItems.Should().HaveCount(1);
        restaurant.MenuItems.First().Should().Be(menuItem);
        
        var addedEvent = restaurant.DomainEvents.OfType<MenuItemAddedDomainEvent>().FirstOrDefault();
        addedEvent.Should().NotBeNull();
        addedEvent!.MenuItemId.Should().Be(menuItem.Id);
    }

    [Fact]
    public void AddMenuItem_WithMultipleItems_Should_AddAllItems()
    {
        // Arrange
        var restaurant = CreateTestRestaurant();

        // Act
        restaurant.AddMenuItem("Margherita Pizza", "Classic pizza", 12.99m, "Pizza");
        restaurant.AddMenuItem("Pepperoni Pizza", "Spicy pizza", 14.99m, "Pizza");
        restaurant.AddMenuItem("Caesar Salad", "Fresh salad", 8.99m, "Salad");

        // Assert
        restaurant.MenuItems.Should().HaveCount(3);
        restaurant.DomainEvents.OfType<MenuItemAddedDomainEvent>().Should().HaveCount(3);
    }

    [Fact]
    public void UpdateMenuItem_Should_UpdateExistingItem()
    {
        // Arrange
        var restaurant = CreateTestRestaurant();
        var menuItem = restaurant.AddMenuItem("Margherita Pizza", "Classic pizza", 12.99m, "Pizza");

        // Act
        restaurant.UpdateMenuItem(menuItem.Id, "Margherita Pizza", "Updated description", 13.99m, "Pizza", "new-image.jpg");

        // Assert
        menuItem.Description.Should().Be("Updated description");
        menuItem.Price.Should().Be(13.99m);
        menuItem.ImageUrl.Should().Be("new-image.jpg");
        
        var updatedEvent = restaurant.DomainEvents.OfType<MenuItemUpdatedDomainEvent>().FirstOrDefault();
        updatedEvent.Should().NotBeNull();
    }

    [Fact]
    public void UpdateMenuItem_WithNonExistentItem_Should_ThrowInvalidOperationException()
    {
        // Arrange
        var restaurant = CreateTestRestaurant();
        var nonExistentId = Guid.NewGuid();

        // Act
        var act = () => restaurant.UpdateMenuItem(nonExistentId, "Name", "Desc", 10m, "Cat");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"Menu item {nonExistentId} not found");
    }

    [Fact]
    public void RemoveMenuItem_Should_RemoveItemFromMenu()
    {
        // Arrange
        var restaurant = CreateTestRestaurant();
        var menuItem = restaurant.AddMenuItem("Margherita Pizza", "Classic pizza", 12.99m, "Pizza");

        // Act
        restaurant.RemoveMenuItem(menuItem.Id);

        // Assert
        restaurant.MenuItems.Should().BeEmpty();
        
        var removedEvent = restaurant.DomainEvents.OfType<MenuItemRemovedDomainEvent>().FirstOrDefault();
        removedEvent.Should().NotBeNull();
        removedEvent!.MenuItemId.Should().Be(menuItem.Id);
    }

    [Fact]
    public void RemoveMenuItem_WithNonExistentItem_Should_ThrowInvalidOperationException()
    {
        // Arrange
        var restaurant = CreateTestRestaurant();
        var nonExistentId = Guid.NewGuid();

        // Act
        var act = () => restaurant.RemoveMenuItem(nonExistentId);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"Menu item {nonExistentId} not found");
    }

    [Fact]
    public void SetMenuItemAvailability_Should_UpdateItemAvailability()
    {
        // Arrange
        var restaurant = CreateTestRestaurant();
        var menuItem = restaurant.AddMenuItem("Margherita Pizza", "Classic pizza", 12.99m, "Pizza");

        // Act
        restaurant.SetMenuItemAvailability(menuItem.Id, false);

        // Assert
        menuItem.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public void SetMenuItemAvailability_WithNonExistentItem_Should_ThrowInvalidOperationException()
    {
        // Arrange
        var restaurant = CreateTestRestaurant();
        var nonExistentId = Guid.NewGuid();

        // Act
        var act = () => restaurant.SetMenuItemAvailability(nonExistentId, false);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"Menu item {nonExistentId} not found");
    }

    [Fact]
    public void IsOpen_DuringOperatingHours_Should_ReturnTrue()
    {
        // Arrange
        var restaurant = CreateTestRestaurant();
        restaurant.Activate();
        var testTime = new DateTime(2025, 12, 28, 15, 0, 0); // 3 PM

        // Act
        var isOpen = restaurant.IsOpen(testTime);

        // Assert
        isOpen.Should().BeTrue();
    }

    [Fact]
    public void IsOpen_BeforeOpeningHours_Should_ReturnFalse()
    {
        // Arrange
        var restaurant = CreateTestRestaurant();
        restaurant.Activate();
        var testTime = new DateTime(2025, 12, 28, 8, 0, 0); // 8 AM

        // Act
        var isOpen = restaurant.IsOpen(testTime);

        // Assert
        isOpen.Should().BeFalse();
    }

    [Fact]
    public void IsOpen_AfterClosingHours_Should_ReturnFalse()
    {
        // Arrange
        var restaurant = CreateTestRestaurant();
        restaurant.Activate();
        var testTime = new DateTime(2025, 12, 28, 23, 0, 0); // 11 PM

        // Act
        var isOpen = restaurant.IsOpen(testTime);

        // Assert
        isOpen.Should().BeFalse();
    }

    [Fact]
    public void IsOpen_WhenRestaurantClosed_Should_ReturnFalse()
    {
        // Arrange
        var restaurant = CreateTestRestaurant();
        var testTime = new DateTime(2025, 12, 28, 15, 0, 0); // 3 PM (within hours but restaurant closed)

        // Act
        var isOpen = restaurant.IsOpen(testTime);

        // Assert
        isOpen.Should().BeFalse();
    }

    [Fact]
    public void UpdateInfo_Should_UpdateAllProperties()
    {
        // Arrange
        var restaurant = CreateTestRestaurant();
        var newOpeningTime = new TimeSpan(10, 0, 0);
        var newClosingTime = new TimeSpan(23, 0, 0);

        // Act
        restaurant.UpdateInfo(
            "New Name", "New Description", "New Address", "555-9999",
            newOpeningTime, newClosingTime, 6.99m, 20.00m, 45);

        // Assert
        restaurant.Name.Should().Be("New Name");
        restaurant.Description.Should().Be("New Description");
        restaurant.Address.Should().Be("New Address");
        restaurant.PhoneNumber.Should().Be("555-9999");
        restaurant.OpeningTime.Should().Be(newOpeningTime);
        restaurant.ClosingTime.Should().Be(newClosingTime);
        restaurant.DeliveryFee.Should().Be(6.99m);
        restaurant.MinimumOrderAmount.Should().Be(20.00m);
        restaurant.EstimatedPreparationTimeMinutes.Should().Be(45);
    }

    private static RestaurantEntity CreateTestRestaurant()
    {
        return new RestaurantEntity(
            RestaurantName, Description, Address, PhoneNumber,
            new TimeSpan(9, 0, 0), new TimeSpan(22, 0, 0), 5.99m, 15.00m, 30);
    }
}
