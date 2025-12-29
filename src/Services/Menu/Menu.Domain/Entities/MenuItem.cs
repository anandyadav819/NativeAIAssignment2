using Common.Domain;

namespace Menu.Domain.Entities;

/// <summary>
/// Menu item entity
/// </summary>
public class MenuItem : Entity<Guid>
{
    public Guid RestaurantId { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public decimal Price { get; private set; }
    public string Category { get; private set; }
    public string? ImageUrl { get; private set; }
    public bool IsAvailable { get; private set; }

    private MenuItem() 
    {
        Name = string.Empty;
        Description = string.Empty;
        Category = string.Empty;
    } // EF Core

    public MenuItem(
        Guid restaurantId,
        string name,
        string description,
        decimal price,
        string category,
        string? imageUrl = null)
    {
        Id = Guid.NewGuid();
        RestaurantId = restaurantId;
        Name = name;
        Description = description;
        Price = price;
        Category = category;
        ImageUrl = imageUrl;
        IsAvailable = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(
        string name,
        string description,
        decimal price,
        string category,
        string? imageUrl = null)
    {
        Name = name;
        Description = description;
        Price = price;
        Category = category;
        ImageUrl = imageUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetAvailability(bool isAvailable)
    {
        IsAvailable = isAvailable;
        UpdatedAt = DateTime.UtcNow;
    }
}
