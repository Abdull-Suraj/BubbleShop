namespace BubbleShop.Application.DTOs;

public class ChannelStatusDto
{
    public Guid Id { get; set; }
    public Guid BusinessId { get; set; }
    public string BusinessName { get; set; } = string.Empty;
    public string ChannelType { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsVerified { get; set; }
    public string? WebhookUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastActiveAt { get; set; }
    public int TotalMessagesReceived { get; set; }
    public int TotalMessagesSent { get; set; }
    public int TotalConversations { get; set; }
    public double AverageResponseTime { get; set; }
    public string Status { get; set; } = string.Empty;
}