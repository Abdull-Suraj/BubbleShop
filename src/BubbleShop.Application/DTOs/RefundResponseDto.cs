namespace BubbleShop.Application.DTOs;

public class RefundResponseDto
{
    public Guid PaymentId { get; set; }
    public string TransactionReference { get; set; } = string.Empty;
    public decimal RefundAmount { get; set; }
    public DateTime RefundedAt { get; set; }
    public string NewStatus { get; set; } = string.Empty;
    public decimal RemainingRefundable { get; set; }
}