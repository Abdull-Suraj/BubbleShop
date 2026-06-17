// Domain/Enums/PaymentStatus.cs
namespace BubbleShop.Domain.Enums;

public enum PaymentStatus
{
    Pending = 0,
    Processing = 1,      // Add this
    Successful = 2,      // Rename from Completed
    Failed = 3,
    Refunded = 4,

}