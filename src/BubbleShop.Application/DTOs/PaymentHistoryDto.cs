namespace BubbleShop.Application.DTOs;

public class PaymentHistoryDto
{
    public Guid Id { get; set; }
    public string TransactionReference { get; set; } = string.Empty;
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal AmountRefunded { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal PlatformFee { get; set; }
    public decimal PaymentGatewayFee { get; set; }
    public decimal BusinessEarnings { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
}