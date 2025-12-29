using Common.Contracts.Enums;
using Common.Domain;
using Menu.Domain.Events;

namespace Menu.Domain.Entities;

/// <summary>
/// Restaurant aggregate root
/// </summary>
public class Restaurant : AggregateRoot<Guid>
{
    private readonly List<MenuItem> _menuItems = new();

    public string Name { get; private set; }
    public string Description { get; private set; }
    public string Address { get; private set; }
    public string PhoneNumber { get; private set; }
    public RestaurantStatus Status { get; private set; }
    public TimeSpan OpeningTime { get; private set; }
    public TimeSpan ClosingTime { get; private set; }
    public decimal DeliveryFee { get; private set; }
    public decimal MinimumOrderAmount { get; private set; }
    public int EstimatedPreparationTimeMinutes { get; private set; }
    public IReadOnlyList<MenuItem> MenuItems => _menuItems.AsReadOnly();

    private Restaurant() 
    {
        Name = string.Empty;
        Description = string.Empty;
        Address = string.Empty;
        PhoneNumber = string.Empty;
    } // EF Core

    public Restaurant(
        string name,
        string description,
        string address,
        string phoneNumber,
        TimeSpan openingTime,
        TimeSpan closingTime,
        decimal deliveryFee,
        decimal minimumOrderAmount,
        int estimatedPreparationTimeMinutes)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        Address = address;
        PhoneNumber = phoneNumber;
        Status = RestaurantStatus.Closed;
        OpeningTime = openingTime;
        ClosingTime = closingTime;
        DeliveryFee = deliveryFee;
        MinimumOrderAmount = minimumOrderAmount;
        EstimatedPreparationTimeMinutes = estimatedPreparationTimeMinutes;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new RestaurantCreatedDomainEvent(Id, Name));
    }

    public void UpdateInfo(
        string name,
        string description,
        string address,
        string phoneNumber,
        TimeSpan openingTime,
        TimeSpan closingTime,
        decimal deliveryFee,
        decimal minimumOrderAmount,
        int estimatedPreparationTimeMinutes)
    {
        Name = name;
        Description = description;
        Address = address;
        PhoneNumber = phoneNumber;
        OpeningTime = openingTime;
        ClosingTime = closingTime;
        DeliveryFee = deliveryFee;
        MinimumOrderAmount = minimumOrderAmount;
        EstimatedPreparationTimeMinutes = estimatedPreparationTimeMinutes;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        if (Status == RestaurantStatus.Open)
            throw new InvalidOperationException("Restaurant is already open");

        Status = RestaurantStatus.Open;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new RestaurantStatusChangedDomainEvent(Id, Status));
    }

    public void Deactivate()
    {
        if (Status == RestaurantStatus.Closed)
            throw new InvalidOperationException("Restaurant is already closed");

        Status = RestaurantStatus.Closed;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new RestaurantStatusChangedDomainEvent(Id, Status));
    }

    public void Suspend()
    {
        Status = RestaurantStatus.TemporarilyClosed;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new RestaurantStatusChangedDomainEvent(Id, Status));
    }

    public MenuItem AddMenuItem(
        string name,
        string description,
        decimal price,
        string category,
        string? imageUrl = null)
    {
        var menuItem = new MenuItem(Id, name, description, price, category, imageUrl);
        _menuItems.Add(menuItem);
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new MenuItemAddedDomainEvent(Id, menuItem.Id, name));
        return menuItem;
    }

    public void UpdateMenuItem(
        Guid menuItemId,
        string name,
        string description,
        decimal price,
        string category,
        string? imageUrl = null)
    {
        var menuItem = _menuItems.FirstOrDefault(m => m.Id == menuItemId);
        if (menuItem == null)
            throw new InvalidOperationException($"Menu item {menuItemId} not found");

        menuItem.Update(name, description, price, category, imageUrl);
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new MenuItemUpdatedDomainEvent(Id, menuItemId, name));
    }

    public void RemoveMenuItem(Guid menuItemId)
    {
        var menuItem = _menuItems.FirstOrDefault(m => m.Id == menuItemId);
        if (menuItem == null)
            throw new InvalidOperationException($"Menu item {menuItemId} not found");

        _menuItems.Remove(menuItem);
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new MenuItemRemovedDomainEvent(Id, menuItemId));
    }

    public void SetMenuItemAvailability(Guid menuItemId, bool isAvailable)
    {
        var menuItem = _menuItems.FirstOrDefault(m => m.Id == menuItemId);
        if (menuItem == null)
            throw new InvalidOperationException($"Menu item {menuItemId} not found");

        menuItem.SetAvailability(isAvailable);
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsOpen(DateTime currentTime)
    {
        var currentTimeOfDay = currentTime.TimeOfDay;
        return Status == RestaurantStatus.Open &&
               currentTimeOfDay >= OpeningTime &&
               currentTimeOfDay <= ClosingTime;
    }
}
