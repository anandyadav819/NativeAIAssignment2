using Common.Domain;

namespace Order.Domain.Entities;

/// <summary>
/// Order item entity
/// </summary>
public class OrderItem : Entity<Guid>
{
    public Guid OrderId { get; private set; }
    public Guid MenuItemId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    // For EF Core
    private OrderItem() { }

    public OrderItem(Guid orderId, Guid menuItemId, string name, int quantity, decimal unitPrice)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        MenuItemId = menuItemId;
        Name = name;
        Quantity = quantity;
        UnitPrice = unitPrice;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

        Quantity = quantity;
    }
}
