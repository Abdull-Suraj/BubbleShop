namespace BubbleShop.Application.DTOs;

public class PaymentStatusDto
{
    public Guid Id { get; set; }
    public string TransactionReference { get; set; } = string.Empty;
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal AmountRefunded { get; set; }
    public string Status { get; set; } = string.Empty;
    public string StatusDescription { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? RefundedAt { get; set; }
}