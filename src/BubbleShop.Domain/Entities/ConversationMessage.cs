using BubbleShop.Domain.Common;

namespace BubbleShop.Domain.Entities;

public class ConversationMessage : BaseEntity
{
    public Guid ConversationId { get; private set; }

    public string Message { get; private set; } = string.Empty;

    public string Sender { get; private set; } = string.Empty;

    public bool IsFromCustomer { get; private set; }

    public bool IsRead { get; private set; }

    public DateTime Timestamp { get; private set; }

    // Navigation Property
    public Conversation Conversation { get; private set; } = null!;

    private ConversationMessage()
    {
    }

    public ConversationMessage(
        Guid conversationId,
        string message,
        string sender,
        bool isFromCustomer)
    {
        Id = Guid.NewGuid();
        ConversationId = conversationId;
        Message = message;
        Sender = sender;
        IsFromCustomer = isFromCustomer;
        Timestamp = DateTime.UtcNow;
        IsRead = !isFromCustomer;
    }

    public void MarkAsRead()
    {
        IsRead = true;
        LastModifiedAt = DateTime.UtcNow;
    }
}