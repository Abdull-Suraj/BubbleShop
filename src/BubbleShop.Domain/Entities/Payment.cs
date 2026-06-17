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
    public Guid Id { get; private set; }
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

    // ADD THIS METHOD
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

    public void Refund(decimal refundAmount, string? reason = null)
    {
        if (Status != PaymentStatus.Successful)
            throw new DomainException("Only successful payments can be refunded");

        if (refundAmount <= 0)
            throw new DomainException("Refund amount must be positive");

        if (AmountRefunded + refundAmount > AmountPaid)
            throw new DomainException("Refund amount exceeds paid amount");

        AmountRefunded += refundAmount;

        if (AmountRefunded >= AmountPaid)
        {
            Status = PaymentStatus.Refunded;
            RefundedAt = DateTime.UtcNow;
        }

    }



    public void UpdateBillingAddress(string address)
    {
        // Add billing address if needed
    }

    public void AddMetadata(string key, string value)
    {
        // Add metadata if needed
    }

    // Helper Methods
    public decimal GetRemainingBalance() => Amount - AmountPaid;
    public decimal GetRefundableAmount() => AmountPaid - AmountRefunded;
    public bool IsFullyPaid => AmountPaid >= Amount;
}