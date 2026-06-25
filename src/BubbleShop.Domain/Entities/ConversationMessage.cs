using BubbleShop.Domain.Common;

namespace BubbleShop.Domain.Entities;

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

    public string Message { get; private set; } = string.Empty;

    public string Sender { get; private set; } = string.Empty;

    public bool IsFromCustomer { get; private set; }

    public DateTime Timestamp { get; private set; }
}