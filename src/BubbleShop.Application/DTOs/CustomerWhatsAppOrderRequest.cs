
namespace BubbleShop.Application.DTOs;

public class CustomerWhatsAppOrderRequest
{
    public string CustomerWhatsApp { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string BusinessWhatsApp { get; set; } = string.Empty;  // The store's WhatsApp number
    public string Message { get; set; } = string.Empty;
}