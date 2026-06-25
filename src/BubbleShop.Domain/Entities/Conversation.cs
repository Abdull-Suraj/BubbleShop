using BubbleShop.Domain.Common;
using BubbleShop.Domain.Enums;

namespace BubbleShop.Domain.Entities;

public sealed class Conversation : BaseEntity
{
    private Conversation() { }

    public Conversation(
        Guid businessId,
        Guid customerId,
        string whatsAppNumber,
        string customerName)
    {
        Id = Guid.NewGuid();
        BusinessId = businessId;
        CustomerId = customerId;
        WhatsAppNumber = whatsAppNumber;
        CustomerName = customerName;

        Status = ConversationStatus.Active;

        CreatedAt = DateTime.UtcNow;
        LastMessageAt = DateTime.UtcNow;
    }

    public Guid BusinessId { get; private set; }

    public Guid CustomerId { get; private set; }

    public string WhatsAppNumber { get; private set; } = string.Empty;

    public string CustomerName { get; private set; } = string.Empty;

    public ConversationStatus Status { get; private set; }

    public DateTime? LastMessageAt { get; private set; }

    public int UnreadCount { get; private set; }

    public ICollection<ConversationMessage> Messages { get; private set; }
        = new List<ConversationMessage>();

    public Business? Business { get; private set; }

    public Customer? Customer { get; private set; }

    public void AddCustomerMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        Messages.Add(new ConversationMessage(
            message,
            "Customer",
            true));

        LastMessageAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
        UnreadCount++;
    }

    public void AddAssistantMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        Messages.Add(new ConversationMessage(
            message,
            "Assistant",
            false));

        LastMessageAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void AddSystemMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        Messages.Add(new ConversationMessage(
            message,
            "System",
            false));

        LastMessageAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void MarkAsRead()
    {
        UnreadCount = 0;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Close()
    {
        Status = ConversationStatus.Closed;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Reopen()
    {
        Status = ConversationStatus.Active;
        LastModifiedAt = DateTime.UtcNow;
    }

    public List<ChatMessage> ToChatHistory()
    {
        return Messages
            .OrderBy(x => x.Timestamp)
            .Select(x => new ChatMessage
            {
                Role = x.IsFromCustomer
                    ? ChatRole.User
                    : ChatRole.Assistant,
                Content = x.Message,
                Timestamp = x.Timestamp
            })
            .ToList();
    }

    public override string ToString()
    {
        return $"{CustomerName} ({WhatsAppNumber})";
    }
}