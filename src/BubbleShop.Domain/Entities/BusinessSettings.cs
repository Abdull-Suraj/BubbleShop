namespace BubbleShop.Domain.Entities;

public class BusinessSettings 
{
    
    public Guid Id { get; set; }
    public bool AutoConfirmOrders { get; set; } = true;
    public bool AutoSendReceipts { get; set; } = true;
    public bool EnableWhatsAppNotifications { get; set; } = true;
    public bool EnableEmailNotifications { get; set; } = true;
    public bool EnableSMSNotifications { get; set; } = false;
    public int DefaultDeliveryTimeInHours { get; set; } = 48;
    public string WelcomeMessage { get; set; } = "Welcome to our store! How can we help you today?";
    public string OrderConfirmationMessage { get; set; } = "Thank you for your order! Your order number is {OrderNumber}";
    public string OrderCancellationMessage { get; set; } = "Your order has been cancelled.";
    public WorkingHours WorkingHours { get; set; } = new();
    public List<string> SupportedPaymentMethods { get; set; } = new() { "Card", "Transfer" };
    //public bool IsPickupAvailable { get; set; } = true;
    //public bool IsDeliveryAvailable { get; set; } = true;
}
