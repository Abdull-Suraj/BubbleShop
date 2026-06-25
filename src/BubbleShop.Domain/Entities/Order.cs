using BubbleShop.Domain.Common;
using BubbleShop.Domain.DomainEvents;
using BubbleShop.Domain.Enums;
using BubbleShop.Domain.Exceptions;

namespace BubbleShop.Domain.Entities;

public sealed class Order : BaseEntity
{
    private Order()
    {
    }

    private Order(Guid BusinessId, Guid customerId, IEnumerable<OrderItem> orderItems, string? customerName = null, string? customerWhatsApp = null)
    {
        Id = Guid.NewGuid();
        OrderNumber = GenerateOrderNumber();
        CustomerId = customerId;
        BusinessId = BusinessId;  // Initialize BusinessId
        CustomerName = customerName ?? "Customer";
        CustomerWhatsApp = customerWhatsApp ?? string.Empty;
        Status = OrderStatus.Pending;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
        OrderItems = orderItems.ToList();
        TotalAmount = OrderItems.Sum(x => x.Quantity * x.UnitPrice);
        AddDomainEvent(new OrderPlacedEvent(this));
    }

    
    public Guid BusinessId { get; private set; }  // ADD THIS
    public string OrderNumber { get; private set; } = string.Empty;

    public Guid CustomerId { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;  // ADD THIS
    public string CustomerWhatsApp { get; private set; } = string.Empty;  // ADD THIS
    public string CustomerEmail { get; private set; } = string.Empty;
    public string CustomerPhone { get; private set; } = string.Empty;
    public string ShippingAddress { get; private set; } = string.Empty;
    public string BillingAddress { get; private set; } = string.Empty;
    public OrderStatus Status { get; private set; }
    public decimal Subtotal { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal DeliveryFee { get; private set; }
    public decimal TotalAmount { get; private set; }
 
    public DateTime UpdatedAt { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public DateTime? ConfirmedAt { get; private set; }
    public DateTime? ShippedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public ICollection<OrderItem> OrderItems { get; private set; } = [];
    public Payment? Payment { get; private set; }
    public Delivery? Delivery { get; private set; }
    public string? CancellationReason { get; private set; }

    // Factory Methods
    public static Order Create(Guid businessId, Guid customerId, IEnumerable<OrderItem> items, string? customerName = null, string? customerWhatsApp = null)
    {
        var orderItems = items.ToList();
        if (orderItems.Count == 0)
        {
            throw new DomainException("Order must contain at least one item.");
        }

        return new Order(businessId, customerId, orderItems, customerName, customerWhatsApp);
    }

    private static string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid():N}"[..8].ToUpper();
    }

    public void UpdateStatus(OrderStatus status)
    {
        if (Status == OrderStatus.Cancelled && status != OrderStatus.Cancelled)
        {
            throw new InvalidOrderStateException("Cancelled orders cannot transition to another state.");
        }

        Status = status;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Confirm() => UpdateStatus(OrderStatus.Confirmed);

    public void Cancel()
    {
        if (Status == OrderStatus.Delivered)
        {
            throw new InvalidOrderStateException("Delivered orders cannot be cancelled.");
        }

        UpdateStatus(OrderStatus.Cancelled);
        CancelledAt = DateTime.UtcNow;
        CancellationReason = "Cancelled by customer";
    }

    public void Cancel(string? reason)
    {
        if (Status == OrderStatus.Delivered || Status == OrderStatus.Completed)
        {
            throw new InvalidOrderStateException("Delivered or completed orders cannot be cancelled.");
        }

        if (Status == OrderStatus.Shipped)
        {
            throw new InvalidOrderStateException("Shipped orders cannot be cancelled. Please contact support.");
        }

        UpdateStatus(OrderStatus.Cancelled);
        CancelledAt = DateTime.UtcNow;
        CancellationReason = reason ?? "Cancelled by customer";

        AddDomainEvent(new OrderCancelledEvent(Id, OrderNumber, CancellationReason));
    }

    public void AttachPayment(Payment payment) => Payment = payment;

    public void AttachDelivery(Delivery delivery) => Delivery = delivery;

    public bool CanBeCancelled()
    {
        return Status != OrderStatus.Delivered &&
               Status != OrderStatus.Completed &&
               Status != OrderStatus.Shipped;
    }
}