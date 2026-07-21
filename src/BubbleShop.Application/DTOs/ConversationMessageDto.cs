
namespace BubbleShop.Application.DTOs;

public class ConversationMessageDto
{
    public Guid Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Sender { get; set; } = string.Empty;
    public bool IsFromCustomer { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsRead { get; set; }
}