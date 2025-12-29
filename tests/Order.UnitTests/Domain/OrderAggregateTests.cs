using Common.Contracts.Enums;
using FluentAssertions;
using Order.Domain.Events;
using OrderEntity = Order.Domain.Entities.Order;

namespace Order.UnitTests.Domain;

public class OrderAggregateTests
{
    private readonly Guid _customerId = Guid.NewGuid();
    private readonly Guid _restaurantId = Guid.NewGuid();
    private const string DeliveryAddress = "123 Main St, City";

    [Fact]
    public void Constructor_Should_CreateOrderWithPendingStatus()
    {
        // Act
        var order = new OrderEntity(_customerId, _restaurantId, DeliveryAddress);

        // Assert
        order.Id.Should().NotBeEmpty();
        order.CustomerId.Should().Be(_customerId);
        order.RestaurantId.Should().Be(_restaurantId);
        order.DeliveryAddress.Should().Be(DeliveryAddress);
        order.Status.Should().Be(OrderStatus.Pending);
        order.TotalAmount.Should().Be(0);
        order.Version.Should().Be(1);
        order.Items.Should().BeEmpty();
        order.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<OrderCreatedDomainEvent>();
    }

    [Fact]
    public void AddItem_Should_AddItemAndRecalculateTotalAmount()
    {
        // Arrange
        var order = new OrderEntity(_customerId, _restaurantId, DeliveryAddress);
        var menuItemId = Guid.NewGuid();

        // Act
        order.AddItem(menuItemId, "Pizza", 2, 15.99m);

        // Assert
        order.Items.Should().HaveCount(1);
        order.Items.First().MenuItemId.Should().Be(menuItemId);
        order.Items.First().Name.Should().Be("Pizza");
        order.Items.First().Quantity.Should().Be(2);
        order.Items.First().UnitPrice.Should().Be(15.99m);
        order.TotalAmount.Should().Be(31.98m);
    }

    [Fact]
    public void AddItem_WithMultipleItems_Should_CalculateCorrectTotalAmount()
    {
        // Arrange
        var order = new OrderEntity(_customerId, _restaurantId, DeliveryAddress);

        // Act
        order.AddItem(Guid.NewGuid(), "Pizza", 2, 15.99m);
        order.AddItem(Guid.NewGuid(), "Burger", 1, 9.99m);
        order.AddItem(Guid.NewGuid(), "Coke", 3, 2.50m);

        // Assert
        order.Items.Should().HaveCount(3);
        order.TotalAmount.Should().Be(49.47m); // (2*15.99) + (1*9.99) + (3*2.50) = 31.98 + 9.99 + 7.50
    }

    [Fact]
    public void AddItem_WithZeroQuantity_Should_ThrowArgumentException()
    {
        // Arrange
        var order = new OrderEntity(_customerId, _restaurantId, DeliveryAddress);

        // Act
        var act = () => order.AddItem(Guid.NewGuid(), "Pizza", 0, 15.99m);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Quantity must be greater than zero*");
    }

    [Fact]
    public void AddItem_WithNegativePrice_Should_ThrowArgumentException()
    {
        // Arrange
        var order = new OrderEntity(_customerId, _restaurantId, DeliveryAddress);

        // Act
        var act = () => order.AddItem(Guid.NewGuid(), "Pizza", 2, -5.00m);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Unit price cannot be negative*");
    }

    [Fact]
    public void AddItem_WhenOrderIsNotPending_Should_ThrowInvalidOperationException()
    {
        // Arrange
        var order = new OrderEntity(_customerId, _restaurantId, DeliveryAddress);
        order.AddItem(Guid.NewGuid(), "Pizza", 1, 15.99m);
        order.ConfirmPayment("TXN123");

        // Act
        var act = () => order.AddItem(Guid.NewGuid(), "Burger", 1, 9.99m);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot add items to an order that is not pending");
    }

    [Fact]
    public void RemoveItem_Should_RemoveItemAndRecalculateTotalAmount()
    {
        // Arrange
        var order = new OrderEntity(_customerId, _restaurantId, DeliveryAddress);
        order.AddItem(Guid.NewGuid(), "Pizza", 2, 15.99m);
        order.AddItem(Guid.NewGuid(), "Burger", 1, 9.99m);
        var itemToRemove = order.Items.First();

        // Act
        order.RemoveItem(itemToRemove.Id);

        // Assert
        order.Items.Should().HaveCount(1);
        order.TotalAmount.Should().Be(9.99m);
    }

    [Fact]
    public void RemoveItem_WhenOrderIsNotPending_Should_ThrowInvalidOperationException()
    {
        // Arrange
        var order = new OrderEntity(_customerId, _restaurantId, DeliveryAddress);
        order.AddItem(Guid.NewGuid(), "Pizza", 1, 15.99m);
        var itemId = order.Items.First().Id;
        order.ConfirmPayment("TXN123");

        // Act
        var act = () => order.RemoveItem(itemId);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot remove items from an order that is not pending");
    }

    [Fact]
    public void ConfirmPayment_Should_UpdateStatusAndAddTransactionId()
    {
        // Arrange
        var order = new OrderEntity(_customerId, _restaurantId, DeliveryAddress);
        order.AddItem(Guid.NewGuid(), "Pizza", 1, 15.99m);
        const string transactionId = "TXN123456";

        // Act
        order.ConfirmPayment(transactionId);

        // Assert
        order.Status.Should().Be(OrderStatus.PaymentConfirmed);
        order.PaymentTransactionId.Should().Be(transactionId);
        order.Version.Should().Be(2);
        order.DomainEvents.Should().Contain(e => e is OrderPaymentConfirmedDomainEvent);
    }

    [Fact]
    public void ConfirmPayment_WhenOrderIsNotInValidStatus_Should_ThrowInvalidOperationException()
    {
        // Arrange
        var order = new OrderEntity(_customerId, _restaurantId, DeliveryAddress);
        order.AddItem(Guid.NewGuid(), "Pizza", 1, 15.99m);
        order.ConfirmPayment("TXN123");
        order.Confirm();

        // Act
        var act = () => order.ConfirmPayment("TXN456");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot confirm payment for order in Confirmed status");
    }

    [Fact]
    public void Cancel_Should_UpdateStatusAndRaiseEvent()
    {
        // Arrange
        var order = new OrderEntity(_customerId, _restaurantId, DeliveryAddress);
        order.AddItem(Guid.NewGuid(), "Pizza", 1, 15.99m);
        const string reason = "Customer requested cancellation";

        // Act
        order.Cancel(reason);

        // Assert
        order.Status.Should().Be(OrderStatus.Cancelled);
        order.Version.Should().Be(2);
        var cancelEvent = order.DomainEvents.OfType<OrderCancelledDomainEvent>().FirstOrDefault();
        cancelEvent.Should().NotBeNull();
        cancelEvent!.Reason.Should().Be(reason);
        cancelEvent.PreviousStatus.Should().Be(OrderStatus.Pending);
    }

    [Fact]
    public void AssignDriver_Should_UpdateStatusAndSetDriverId()
    {
        // Arrange
        var order = new OrderEntity(_customerId, _restaurantId, DeliveryAddress);
        order.AddItem(Guid.NewGuid(), "Pizza", 1, 15.99m);
        order.ConfirmPayment("TXN123");
        order.Confirm();
        order.StartPreparing();
        order.MarkReadyForPickup();
        var driverId = Guid.NewGuid();

        // Act
        order.AssignDriver(driverId);

        // Assert
        order.Status.Should().Be(OrderStatus.OutForDelivery);
        order.DriverId.Should().Be(driverId);
        order.Version.Should().Be(6);
        var assignEvent = order.DomainEvents.OfType<OrderAssignedToDriverDomainEvent>().FirstOrDefault();
        assignEvent.Should().NotBeNull();
        assignEvent!.DriverId.Should().Be(driverId);
    }

    [Fact]
    public void OrderLifecycle_CompleteHappyPath_Should_ProgressThroughAllStatuses()
    {
        // Arrange
        var order = new OrderEntity(_customerId, _restaurantId, DeliveryAddress);
        var driverId = Guid.NewGuid();

        // Act & Assert - Complete lifecycle
        order.Status.Should().Be(OrderStatus.Pending);

        order.AddItem(Guid.NewGuid(), "Pizza", 1, 15.99m);
        order.TotalAmount.Should().Be(15.99m);

        order.ConfirmPayment("TXN123");
        order.Status.Should().Be(OrderStatus.PaymentConfirmed);

        order.Confirm();
        order.Status.Should().Be(OrderStatus.Confirmed);

        order.StartPreparing();
        order.Status.Should().Be(OrderStatus.Preparing);

        order.MarkReadyForPickup();
        order.Status.Should().Be(OrderStatus.ReadyForPickup);

        order.AssignDriver(driverId);
        order.Status.Should().Be(OrderStatus.OutForDelivery);
        order.DriverId.Should().Be(driverId);

        order.MarkDelivered();
        order.Status.Should().Be(OrderStatus.Delivered);

        // Verify version incremented throughout lifecycle
        order.Version.Should().Be(7);
    }
}
