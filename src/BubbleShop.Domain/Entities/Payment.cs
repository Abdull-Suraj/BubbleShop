// Domain/Entities/Payment.cs
using BubbleShop.Domain.Common;
using BubbleShop.Domain.Enums;
using BubbleShop.Domain.Exceptions;

namespace BubbleShop.Domain.Entities;

public sealed class Payment : BaseEntity
{
    private Payment()
    {
    }

    public Payment(
        Guid orderId,
        Guid businessId,
        decimal amount,
        PaymentMethod paymentMethod,
        Guid? customerId = null,
        string? provider = null)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        BusinessId = businessId;
        CustomerId = customerId;
        Amount = amount;
        PaymentMethod = paymentMethod;
        Provider = provider ?? string.Empty;
        Status = PaymentStatus.Pending;
        PaymentType = PaymentType.Full;
        TransactionReference = string.Empty;
        AmountPaid = 0;
        AmountRefunded = 0;
        Currency = "NGN";
        CreatedAt = DateTime.UtcNow;
        CalculateFees();
    }

    // Core Identifiers

    public Guid OrderId { get; private set; }
    public Guid BusinessId { get; private set; }
    public Guid? CustomerId { get; private set; }

    // Transaction Identifiers
    public string TransactionReference { get; private set; } = string.Empty;
    public string Provider { get; private set; } = string.Empty;
    public string? TransactionId { get; private set; }

    // Payment Details
    public PaymentStatus Status { get; private set; }
    public PaymentMethod PaymentMethod { get; private set; }
    public PaymentType PaymentType { get; private set; }

    // Amounts
    public decimal Amount { get; private set; }
    public decimal AmountPaid { get; private set; }
    public decimal AmountRefunded { get; private set; }
    public string Currency { get; private set; } = "NGN";

    // Fees & Commissions
    public decimal PlatformFee { get; private set; }
    public decimal PaymentGatewayFee { get; private set; }
    public decimal BusinessEarnings { get; private set; }

    // Payment Timing
    public DateTime? PaidAt { get; private set; }
    public DateTime? RefundedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }

    // Customer Information (snapshot)
    public string? CustomerName { get; private set; }
    public string? CustomerEmail { get; private set; }
    public string? CustomerPhone { get; private set; }

    // Payment Gateway Response
    public string? GatewayResponse { get; private set; }
    public string? FailureReason { get; private set; }

    public Dictionary<string, string> Metadata { get; private set; } = new();

    // Navigation Properties
    public Order? Order { get; private set; }
    public Business? Business { get; private set; }
    public Customer? Customer { get; private set; }

    // Fee Calculation
    private void CalculateFees()
    {
        // Platform fee: 10% of amount
        PlatformFee = Amount * 0.10m;

        // Payment gateway fee: 2.9% + $0.30 (Stripe standard)
        PaymentGatewayFee = (Amount * 0.029m) + 0.30m;

        // Business earnings = Amount - PlatformFee - PaymentGatewayFee
        BusinessEarnings = Amount - PlatformFee - PaymentGatewayFee;

        if (BusinessEarnings < 0) BusinessEarnings = 0;
    }


    public void UpdateTransactionReference(string transactionReference)
    {
        TransactionReference = transactionReference;
    }

    public void MarkSuccessful(string transactionId, string? gatewayResponse = null)
    {
        if (Status != PaymentStatus.Pending && Status != PaymentStatus.Processing)
            throw new DomainException($"Cannot mark payment as successful from {Status} status");

        Status = PaymentStatus.Successful;
        TransactionId = transactionId;
        AmountPaid = Amount;
        PaidAt = DateTime.UtcNow;
        GatewayResponse = gatewayResponse;
    }

    public void MarkProcessing(string? transactionId = null)
    {
        if (Status != PaymentStatus.Pending)
            throw new DomainException($"Cannot mark payment as processing from {Status} status");

        Status = PaymentStatus.Processing;
        TransactionId = transactionId;
    }

    public void MarkFailed(string? failureReason = null)
    {
        if (Status != PaymentStatus.Pending && Status != PaymentStatus.Processing)
            throw new DomainException($"Cannot mark payment as failed from {Status} status");

        Status = PaymentStatus.Failed;
        FailureReason = failureReason;
    }

    //public void Refund(decimal refundAmount, string? reason = null)
    //{
    //    if (Status != PaymentStatus.Successful)
    //        throw new DomainException("Only successful payments can be refunded");

    //    if (refundAmount <= 0)
    //        throw new DomainException("Refund amount must be positive");

    //    if (AmountRefunded + refundAmount > AmountPaid)
    //        throw new DomainException("Refund amount exceeds paid amount");

    //    AmountRefunded += refundAmount;

    //    if (AmountRefunded >= AmountPaid)
    //    {
    //        Status = PaymentStatus.Refunded;
    //        RefundedAt = DateTime.UtcNow;
    //    }

    //}

    public bool IsExpired()
    {
        return ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value && Status == PaymentStatus.Pending;
    }

    public void ExtendExpiry(int hours)
    {
        if (Status != PaymentStatus.Pending)
            throw new DomainException("Only pending payments can be extended");

        ExpiresAt = DateTime.UtcNow.AddHours(hours);
        LastModifiedAt = DateTime.UtcNow;
    }

    // Retry Failed Payment
    //public void Retry()
    //{
    //    if (Status != PaymentStatus.Failed)
    //        throw new DomainException("Only failed payments can be retried");

    //    if (RetryCount >= 3)
    //        throw new DomainException("Maximum retry attempts (3) reached");

    //    Status = PaymentStatus.Pending;
    //    FailureReason = null;
    //    RetryCount++;
    //    LastModifiedAt = DateTime.UtcNow;
    //}

    // Metadata Methods
    public void AddMetadata(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new DomainException("Metadata key cannot be empty");

        Metadata[key] = value;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void RemoveMetadata(string key)
    {
        if (Metadata.ContainsKey(key))
        {
            Metadata.Remove(key);
            LastModifiedAt = DateTime.UtcNow;
        }
    }

    public string? GetMetadata(string key)
    {
        return Metadata.GetValueOrDefault(key);
    }
    //public void UpdateBillingAddress(string address)
    //{
    //    if (Status != PaymentStatus.Pending && Status != PaymentStatus.Processing)
    //        throw new DomainException("Billing address can only be updated for pending or processing payments");

    //    BillingAddress = address;
    //    LastModifiedAt = DateTime.UtcNow;
    //}



    // Helper Methods
    public decimal GetRemainingBalance() => Amount - AmountPaid;
    public decimal GetRefundableAmount() => AmountPaid - AmountRefunded;
    public bool IsFullyPaid => AmountPaid >= Amount;
    public string GetStatusDescription()
    {
        return Status switch
        {
            PaymentStatus.Pending => "Awaiting payment",
            PaymentStatus.Processing => "Processing payment",
            PaymentStatus.Successful => "Payment completed",
            PaymentStatus.Failed => "Payment failed",
            //PaymentStatus.Refunded => "Fully refunded",
          _ => "Unknown status"
        };
    }

    // Generate Transaction Reference
    private static string GenerateTransactionReference()
    {
        return $"TXN-{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..20].ToUpper();
    }

    public override string ToString()
    {
        return $"Payment {TransactionReference} - {Status} - {Amount:C}";
    }
}

