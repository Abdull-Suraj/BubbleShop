// Domain/Common/MessageContext.cs
using BubbleShop.Domain.Enums;

namespace BubbleShop.Domain.Common;

/// <summary>
/// Context information for message processing
/// </summary>
public class MessageContext
{
    public ChannelType Channel { get; set; }

    // External platform user id
    public string ChannelUserId { get; set; } = string.Empty;

    public string ChannelConversationId { get; set; } = string.Empty;

    public Guid BusinessId { get; set; }

    // Internal database customer id
    public Guid? CustomerId { get; set; }

    public string? ConversationId { get; set; }

    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;

    public List<PreviousMessage> ConversationHistory { get; set; } = new();

    public Dictionary<string, string> Metadata { get; set; } = new();

    public string Language { get; set; } = "en";

    public ConversationState? ConversationState { get; set; }
}

/// <summary>
/// Previous message in conversation history
/// </summary>
public class PreviousMessage
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Conversation state for multi-step interactions
/// </summary>
public class ConversationState
{
    public string CurrentStep { get; set; } = string.Empty;
    public Dictionary<string, object> CollectedData { get; set; } = new();
    public Intent PendingIntent { get; set; }
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(30);
}