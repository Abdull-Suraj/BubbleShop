using BubbleShop.Domain.DomainEvents;
using BubbleShop.Domain.Enums;
using BubbleShop.Domain.Exceptions;

namespace BubbleShop.Domain.Entities;

public sealed class Order : BaseEntity
{
    private Order()
    {
    }

    private Order(Guid customerId, IEnumerable<OrderItem> orderItems)
    {
        Id = Guid.NewGuid();
        CustomerId = customerId;
        Status = OrderStatus.Pending;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
        OrderItems = orderItems.ToList();
        TotalAmount = OrderItems.Sum(x => x.Quantity * x.UnitPrice);
        AddDomainEvent(new OrderPlacedEvent(this));
    }

    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public ICollection<OrderItem> OrderItems { get; private set; } = [];
    public Payment? Payment { get; private set; }
    public Delivery? Delivery { get; private set; }

    public static Order Create(Guid customerId, IEnumerable<OrderItem> items)
    {
        var orderItems = items.ToList();
        if (orderItems.Count == 0)
        {
            throw new DomainException("Order must contain at least one item.");
        }

        return new Order(customerId, orderItems);
    }

    public void UpdateStatus(OrderStatus status)
    {
        if (Status == OrderStatus.Cancelled && status != OrderStatus.Cancelled)
        {
            throw new InvalidOrderStateException("Cancelled orders cannot transition to another state.");
        }

        Status = status;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Confirm() => UpdateStatus(OrderStatus.Confirmed);

    public void Cancel()
    {
        if (Status == OrderStatus.Delivered)
        {
            throw new InvalidOrderStateException("Delivered orders cannot be cancelled.");
        }

        UpdateStatus(OrderStatus.Cancelled);
    }

    public void AttachPayment(Payment payment) => Payment = payment;

    public void AttachDelivery(Delivery delivery) => Delivery = delivery;
}
