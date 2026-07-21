
namespace BubbleShop.Application.DTOs;

public class ConversationHistoryDto
{
    public Guid ConversationId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public List<ConversationMessageDto> Messages { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public int TotalMessages { get; set; }
    public int UnreadCount { get; set; }
    public string Status { get; set; } = string.Empty;
}
