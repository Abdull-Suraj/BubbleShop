using BubbleShop.Domain.Enums;

namespace BubbleShop.Domain.Entities;

public sealed class ChatMessage
{
    public ChatRole Role { get; init; }
    public string Content { get; init; } = string.Empty;
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
}
