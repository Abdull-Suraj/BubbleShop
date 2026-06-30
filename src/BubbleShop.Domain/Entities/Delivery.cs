//using BubbleShop.Domain.Enums;

//namespace BubbleShop.Domain.Entities;

//public sealed class Delivery
//{
//    private Delivery()
//    {
//    }

//    private Delivery(Guid orderId, string recipientName, string addressLine1, string? addressLine2, string city, string postcode, string country)
//    {
//        Id = Guid.NewGuid();
//        OrderId = orderId;
//        RecipientName = recipientName;
//        AddressLine1 = addressLine1;
//        AddressLine2 = addressLine2;
//        City = city;
//        Postcode = postcode;
//        Country = country;
//        Status = DeliveryStatus.Pending;
//    }

//    public Guid Id { get; private set; }
//    public Guid OrderId { get; private set; }
//    public string RecipientName { get; private set; } = string.Empty;
//    public string AddressLine1 { get; private set; } = string.Empty;
//    public string? AddressLine2 { get; private set; }
//    public string City { get; private set; } = string.Empty;
//    public string Postcode { get; private set; } = string.Empty;
//    public string Country { get; private set; } = string.Empty;
//    public string? TrackingNumber { get; private set; }
//    public DeliveryStatus Status { get; private set; }
//    public string? Provider { get; private set; }

//    public static Delivery Create(Guid orderId, string recipientName, string addressLine1, string? addressLine2, string city, string postcode, string country)
//        => new(orderId, recipientName, addressLine1, addressLine2, city, postcode, country);

//    public void Arrange(string provider, string trackingNumber)
//    {
//        Provider = provider;
//        TrackingNumber = trackingNumber;
//        Status = DeliveryStatus.Arranged;
//    }
//}
