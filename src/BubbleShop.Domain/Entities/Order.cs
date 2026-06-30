using BubbleShop.Domain.Common;
using BubbleShop.Domain.DomainEvents;
using BubbleShop.Domain.Enums;
using BubbleShop.Domain.Exceptions;
using System.Threading.Channels;

namespace BubbleShop.Domain.Entities;

public sealed class Order : BaseEntity
{
    private Order()
    {
    }

    private Order(Guid businessId, Guid customerId, IEnumerable<OrderItem> orderItems, string? customerName = null, string? customerWhatsApp = null)
    {
        Id = Guid.NewGuid();
        OrderNumber = GenerateOrderNumber();
        CustomerId = customerId;
        BusinessId = businessId;  // Initialize BusinessId
        CustomerName = customerName ?? "Customer";
        CustomerWhatsApp = customerWhatsApp ?? string.Empty;
        Status = OrderStatus.Pending;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
        Channel = "Unknown";
        Metadata = new Dictionary<string, string>();

        OrderItems = orderItems.ToList();
        Subtotal = OrderItems.Sum(x => x.Quantity * x.UnitPrice);
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

    public decimal DeliveryFee { get; private set; }
    public decimal TotalAmount { get; private set; }

    // Channel Information
    public string Channel { get; private set; } = string.Empty;
    public Dictionary<string, string> Metadata { get; private set; } = new();

    public DateTime UpdatedAt { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public DateTime? ConfirmedAt { get; private set; }
    public DateTime? ShippedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public ICollection<OrderItem> OrderItems { get; private set; } = [];
    public Payment? Payment { get; private set; }
    public Business Business { get; private set; } = null!;  // Add this
    public Customer Customer { get; private set; } = null!;  // Add this
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

    public void StartProcessing()
    {
        if (Status != OrderStatus.Confirmed)
        {
            throw new InvalidOrderStateException($"Cannot start processing. Current status: {Status}");
        }
        UpdateStatus(OrderStatus.Processing);
    }

    public void MarkAsShipped()
    {
        if (Status != OrderStatus.Processing)
        {
            throw new InvalidOrderStateException($"Cannot ship order. Current status: {Status}");
        }
        UpdateStatus(OrderStatus.Shipped);
        ShippedAt = DateTime.UtcNow;
    }

    public void MarkAsDelivered()
    {
        if (Status != OrderStatus.Shipped)
        {
            throw new InvalidOrderStateException($"Cannot mark as delivered. Current status: {Status}");
        }
        UpdateStatus(OrderStatus.Delivered);
        DeliveredAt = DateTime.UtcNow;
    }

    public void Complete()
    {
        if (Status != OrderStatus.Delivered)
        {
            throw new InvalidOrderStateException($"Cannot complete order. Current status: {Status}");
        }
        UpdateStatus(OrderStatus.Completed);
        CompletedAt = DateTime.UtcNow;
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

    // Payment Methods
    public void RequestPayment()
    {
        if (Status != OrderStatus.Pending)
        {
            throw new InvalidOrderStateException($"Cannot request payment. Current status: {Status}");
        }
        UpdateStatus(OrderStatus.PaymentPending);
    }

    public void ConfirmPayment(Payment payment)
    {
        if (Status != OrderStatus.PaymentPending && Status != OrderStatus.Pending)
        {
            throw new InvalidOrderStateException($"Cannot confirm payment. Current status: {Status}");
        }
        Payment = payment;
        PaidAt = DateTime.UtcNow;
        UpdateStatus(OrderStatus.PaymentReceived);
    }

    public void AttachPayment(Payment payment) => Payment = payment;

    //public void AttachDelivery(Delivery delivery) => Delivery = delivery;

    // Channel Methods
    public void SetChannel(string channel)
    {
        Channel = channel;
        UpdatedAt = DateTime.UtcNow;
    }

    // Metadata Methods
    public void AddMetadata(string key, string value)
    {
        Metadata[key] = value;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveMetadata(string key)
    {
        if (Metadata.ContainsKey(key))
        {
            Metadata.Remove(key);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public string? GetMetadata(string key)
    {
        return Metadata.GetValueOrDefault(key);
    }

    public bool IsFullyPaid()
    {
        return Payment != null && Payment.IsFullyPaid;
    }

    public decimal GetRemainingBalance()
    {
        if (Payment == null) return TotalAmount;
        return TotalAmount - Payment.AmountPaid;
    }

    public bool HasItems()
    {
        return OrderItems.Any();
    }

    public int GetTotalItemCount()
    {
        return OrderItems.Sum(i => i.Quantity);
    }

    public void AddOrderItem(OrderItem item)
    {
        if (Status != OrderStatus.Pending && Status != OrderStatus.PaymentPending)
        {
            throw new InvalidOrderStateException("Cannot add items to an order that is already processing.");
        }
        OrderItems.Add(item);
        RecalculateTotals();
    }

    public void RemoveOrderItem(Guid orderItemId)
    {
        if (Status != OrderStatus.Pending && Status != OrderStatus.PaymentPending)
        {
            throw new InvalidOrderStateException("Cannot remove items from an order that is already processing.");
        }
        var item = OrderItems.FirstOrDefault(i => i.Id == orderItemId);
        if (item != null)
        {
            OrderItems.Remove(item);
            RecalculateTotals();
        }
    }

    private void RecalculateTotals()
    {
        Subtotal = OrderItems.Sum(x => x.Quantity * x.UnitPrice);
        TotalAmount = Subtotal;
        UpdatedAt = DateTime.UtcNow;
    }

    // Apply Discount


    // Apply Tax
    public bool CanBeCancelled()
    {
        return Status != OrderStatus.Delivered &&
               Status != OrderStatus.Completed &&
               Status != OrderStatus.Shipped;
    }
}