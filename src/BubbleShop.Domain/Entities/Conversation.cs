using BubbleShop.Domain.Common;

namespace BubbleShop.Domain.Entities;

public class Conversation : BaseEntity
{
    private Conversation()
    {
    }

    private Conversation(Guid customerId, string whatsappNumber)
    {
        Id = Guid.NewGuid();
        CustomerId = customerId;
        WhatsAppNumber = whatsappNumber;
        LastUpdated = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public string WhatsAppNumber { get; private set; } = string.Empty;
    public List<ChatMessage> MessageHistory { get; private set; } = [];
    public DateTime LastUpdated { get; private set; }

    public static Conversation Create(Guid customerId, string whatsappNumber) => new(customerId, whatsappNumber);

    public void UpdateHistory(List<ChatMessage> messages)
    {
        MessageHistory = messages;
        LastUpdated = DateTime.UtcNow;
    }
}
