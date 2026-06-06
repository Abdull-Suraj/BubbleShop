namespace BubbleShop.Domain.Entities;

public sealed class OrderItem
{
    private OrderItem()
    {
    }

    private OrderItem(Guid orderId, Guid productId, int quantity, decimal unitPrice)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    public static OrderItem Create(Guid orderId, Guid productId, int quantity, decimal unitPrice)
        => new(orderId, productId, quantity, unitPrice);
}
