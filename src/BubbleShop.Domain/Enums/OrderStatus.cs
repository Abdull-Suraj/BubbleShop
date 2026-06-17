namespace BubbleShop.Domain.Enums;

public enum OrderStatus
{
    Pending,
    PaymentPending,
    PaymentReceived,
    Confirmed,
    Processing,
    Shipped,
    Delivered,
    Completed,
    Cancelled,
    Refunded
}