namespace Common.Contracts.Enums;

public enum OrderStatus
{
    Pending = 0,
    PaymentProcessing = 1,
    PaymentConfirmed = 2,
    PaymentFailed = 3,
    Confirmed = 4,
    Preparing = 5,
    ReadyForPickup = 6,
    OutForDelivery = 7,
    Delivered = 8,
    Cancelled = 9,
    Refunded = 10
}
