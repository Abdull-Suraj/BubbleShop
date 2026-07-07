using BubbleShop.Domain.Common;
using BubbleShop.Domain.Entities;

public sealed class ConversationMessage : BaseEntity
{
    private ConversationMessage() { }

    public ConversationMessage(
        string message,
        string sender,
        bool isFromCustomer)
    {
        Id = Guid.NewGuid();

        Message = message;
        Sender = sender;
        IsFromCustomer = isFromCustomer;
        Timestamp = DateTime.UtcNow;
    }

    public Guid ConversationId { get; private set; }

    public Conversation Conversation { get; private set; } = null!;

    public string Message { get; private set; } = string.Empty;

    public string Sender { get; private set; } = string.Empty;

    public bool IsFromCustomer { get; private set; }

    public DateTime Timestamp { get; private set; }
}