using BubbleShop.Domain.Enums;

namespace BubbleShop.Domain.Entities;

public sealed class Payment
{
    private Payment()
    {
    }

    private Payment(Guid orderId, string provider, decimal amount)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        Provider = provider;
        Amount = amount;
        Status = PaymentStatus.Pending;
    }

    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public string Provider { get; private set; } = string.Empty;
    public PaymentStatus Status { get; private set; }
    public string? TransactionId { get; private set; }
    public decimal Amount { get; private set; }
    public DateTimeOffset? PaidAt { get; private set; }

    public static Payment Create(Guid orderId, string provider, decimal amount) => new(orderId, provider, amount);

    public void MarkCompleted(string transactionId)
    {
        Status = PaymentStatus.Completed;
        TransactionId = transactionId;
        PaidAt = DateTimeOffset.UtcNow;
    }

    public void MarkFailed() => Status = PaymentStatus.Failed;
}
