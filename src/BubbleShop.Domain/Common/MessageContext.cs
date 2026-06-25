// Domain/Common/MessageContext.cs
using BubbleShop.Domain.Enums;

namespace BubbleShop.Domain.Common;

/// <summary>
/// Context information for message processing
/// </summary>
public class MessageContext
{
    /// <summary>
    /// Communication channel (WhatsApp, Telegram, WebChat, etc.)
    /// </summary>
    public ChannelType Channel { get; set; }

    /// <summary>
    /// User ID on the specific channel
    /// </summary>
    public string ChannelUserId { get; set; } = string.Empty;

    /// <summary>
    /// Conversation/Session ID for tracking
    /// </summary>
    public string ChannelConversationId { get; set; } = string.Empty;

    /// <summary>
    /// Business ID this message belongs to
    /// </summary>
    public string BusinessId { get; set; } = string.Empty;

    /// <summary>
    /// Customer ID if already identified (optional)
    /// </summary>
    public Guid? CustomerId { get; set; }

    /// <summary>
    /// Conversation ID for tracking
    /// </summary>
    public string? ConversationId { get; set; }

    /// <summary>
    /// When the message was received
    /// </summary>
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Previous messages in this conversation (for context)
    /// </summary>
    public List<PreviousMessage> ConversationHistory { get; set; } = new();

    /// <summary>
    /// Additional channel-specific metadata
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();

    /// <summary>
    /// Language of the message (auto-detected or specified)
    /// </summary>
    public string Language { get; set; } = "en";

    /// <summary>
    /// Current conversation state (for multi-step interactions)
    /// </summary>
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