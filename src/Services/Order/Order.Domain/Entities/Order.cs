using Common.Contracts.Enums;
using Common.Domain;
using Order.Domain.Events;

namespace Order.Domain.Entities;

/// <summary>
/// Order aggregate root
/// </summary>
public class Order : AggregateRoot<Guid>
{
    private readonly List<OrderItem> _items = new();

    public Guid CustomerId { get; private set; }
    public Guid RestaurantId { get; private set; }
    public OrderStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string? DeliveryAddress { get; private set; }
    public string? PaymentTransactionId { get; private set; }
    public Guid? DriverId { get; private set; }
    public int Version { get; private set; }

    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

    // For EF Core
    private Order() { }

    public Order(Guid customerId, Guid restaurantId, string deliveryAddress)
    {
        Id = Guid.NewGuid();
        CustomerId = customerId;
        RestaurantId = restaurantId;
        DeliveryAddress = deliveryAddress;
        Status = OrderStatus.Pending;
        CreatedAt = DateTime.UtcNow;
        Version = 1;

        AddDomainEvent(new OrderCreatedDomainEvent(Id, customerId, restaurantId));
    }

    public void AddItem(Guid menuItemId, string name, int quantity, decimal unitPrice)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Cannot add items to an order that is not pending");

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative", nameof(unitPrice));

        var item = new OrderItem(Id, menuItemId, name, quantity, unitPrice);
        _items.Add(item);

        RecalculateTotalAmount();
    }

    public void RemoveItem(Guid itemId)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Cannot remove items from an order that is not pending");

        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item != null)
        {
            _items.Remove(item);
            RecalculateTotalAmount();
        }
    }

    public void ConfirmPayment(string transactionId)
    {
        if (Status != OrderStatus.PaymentProcessing && Status != OrderStatus.Pending)
            throw new InvalidOperationException($"Cannot confirm payment for order in {Status} status");

        PaymentTransactionId = transactionId;
        Status = OrderStatus.PaymentConfirmed;
        Version++;

        AddDomainEvent(new OrderPaymentConfirmedDomainEvent(Id, transactionId));
    }

    public void MarkPaymentFailed(string reason)
    {
        if (Status != OrderStatus.PaymentProcessing && Status != OrderStatus.Pending)
            throw new InvalidOperationException($"Cannot mark payment failed for order in {Status} status");

        Status = OrderStatus.PaymentFailed;
        Version++;

        AddDomainEvent(new OrderPaymentFailedDomainEvent(Id, reason));
    }

    public void Confirm()
    {
        if (Status != OrderStatus.PaymentConfirmed)
            throw new InvalidOperationException($"Cannot confirm order in {Status} status");

        Status = OrderStatus.Confirmed;
        Version++;

        AddDomainEvent(new OrderStatusChangedDomainEvent(Id, OrderStatus.PaymentConfirmed, OrderStatus.Confirmed));
    }

    public void StartPreparing()
    {
        if (Status != OrderStatus.Confirmed)
            throw new InvalidOperationException($"Cannot start preparing order in {Status} status");

        Status = OrderStatus.Preparing;
        Version++;

        AddDomainEvent(new OrderStatusChangedDomainEvent(Id, OrderStatus.Confirmed, OrderStatus.Preparing));
    }

    public void MarkReadyForPickup()
    {
        if (Status != OrderStatus.Preparing)
            throw new InvalidOperationException($"Cannot mark order ready in {Status} status");

        Status = OrderStatus.ReadyForPickup;
        Version++;

        AddDomainEvent(new OrderStatusChangedDomainEvent(Id, OrderStatus.Preparing, OrderStatus.ReadyForPickup));
    }

    public void AssignDriver(Guid driverId)
    {
        if (Status != OrderStatus.ReadyForPickup)
            throw new InvalidOperationException($"Cannot assign driver to order in {Status} status");

        DriverId = driverId;
        Status = OrderStatus.OutForDelivery;
        Version++;

        AddDomainEvent(new OrderAssignedToDriverDomainEvent(Id, driverId));
    }

    public void MarkDelivered()
    {
        if (Status != OrderStatus.OutForDelivery)
            throw new InvalidOperationException($"Cannot mark order delivered in {Status} status");

        Status = OrderStatus.Delivered;
        Version++;

        AddDomainEvent(new OrderDeliveredDomainEvent(Id));
    }

    public void Cancel(string reason)
    {
        if (Status == OrderStatus.Delivered || Status == OrderStatus.Cancelled)
            throw new InvalidOperationException($"Cannot cancel order in {Status} status");

        var previousStatus = Status;
        Status = OrderStatus.Cancelled;
        Version++;

        AddDomainEvent(new OrderCancelledDomainEvent(Id, reason, previousStatus));
    }

    private void RecalculateTotalAmount()
    {
        TotalAmount = _items.Sum(i => i.Quantity * i.UnitPrice);
    }
}
