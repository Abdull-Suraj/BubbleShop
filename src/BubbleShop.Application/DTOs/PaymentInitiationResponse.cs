
namespace BubbleShop.Application.DTOs;

public class PaymentInitiationResponse
{
    public string TransactionReference { get; set; } = string.Empty;
    public string PaymentLink { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}