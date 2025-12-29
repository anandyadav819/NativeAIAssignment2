using Common.Contracts.Enums;

namespace Menu.Application.DTOs;

public class RestaurantDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public RestaurantStatus Status { get; set; }
    public TimeSpan OpeningTime { get; set; }
    public TimeSpan ClosingTime { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal MinimumOrderAmount { get; set; }
    public int EstimatedPreparationTimeMinutes { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<MenuItemDto> MenuItems { get; set; } = new();
}

public class MenuItemDto
{
    public Guid Id { get; set; }
    public Guid RestaurantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public bool IsAvailable { get; set; }
}
