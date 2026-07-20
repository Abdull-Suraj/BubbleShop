// Domain/Entities/Conversation.cs
using BubbleShop.Domain.Common;
using BubbleShop.Domain.Enums;

namespace BubbleShop.Domain.Entities;

public class Conversation : BaseEntity
{
    public Guid BusinessId { get; private set; }
    public Guid CustomerId { get; private set; }
    public string WhatsAppNumber { get; private set; } = string.Empty;
    public string CustomerName { get; private set; } = string.Empty;
    public string Channel { get; private set; } = string.Empty;
    public ConversationStatus Status { get; private set; }
   public DateTime? LastMessageAt { get; private set; }
    public int UnreadCount { get; private set; }
    public ICollection<ConversationMessage> Messages { get; private set; }
    = new List<ConversationMessage>();
    
    public string? AssignedAgentId { get; private set; }
    public Dictionary<string, string> Metadata { get; private set; } = new();

    public Business? Business { get; private set; } = null!;
    public Customer? Customer { get; private set; }

    private Conversation() { }

    public Conversation(
        Guid businessId,
        Guid customerId,
        string whatsAppNumber,
        string? customerName = null,
        string channel = "whatsapp")
    {
        Id = Guid.NewGuid();

        BusinessId = businessId;
        CustomerId = customerId;
        WhatsAppNumber = whatsAppNumber;
        CustomerName = customerName ?? "Customer";
        Channel = channel;

        Status = ConversationStatus.Active;
        Messages = new List<ConversationMessage>();

        CreatedAt = DateTime.UtcNow;
        LastMessageAt = DateTime.UtcNow;

        UnreadCount = 0;
        Metadata = new Dictionary<string, string>();
    }

    public void AddMessage(string message, string sender, bool isFromCustomer)
    {
        Messages.Add(new ConversationMessage(
    conversationId: Id,
    message: message,
    sender: sender,
    isFromCustomer: isFromCustomer));
        LastMessageAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;

        if (isFromCustomer)
            UnreadCount++;
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

    public void AssignAgent(string agentId)
    {
        AssignedAgentId = agentId;
        Status = ConversationStatus.Active;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void AddMetadata(string key, string value)
    {
        Metadata[key] = value;
        LastModifiedAt = DateTime.UtcNow;
    }
    public List<ChatMessage> ToChatHistory()
    {
        return Messages
            .OrderBy(m => m.CreatedAt)
            .Select(m => new ChatMessage
            {
                Role = m.IsFromCustomer
                    ? ChatRole.User
                    : ChatRole.Assistant,

                Content = m.Message,

                Timestamp = m.CreatedAt
            })
            .ToList();
    }
}

