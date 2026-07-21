namespace BubbleShop.Application.DTOs;

public class ChannelDto
{
    public Guid Id { get; set; }
    public Guid BusinessId { get; set; }
    public string ChannelType { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsVerified { get; set; }
    public string? WebhookUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastActiveAt { get; set; }
}