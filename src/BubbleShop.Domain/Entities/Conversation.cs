namespace BubbleShop.Domain.Entities;

public sealed class Conversation
{
    private Conversation()
    {
    }

    private Conversation(Guid customerId, string whatsappNumber)
    {
        Id = Guid.NewGuid();
        CustomerId = customerId;
        WhatsAppNumber = whatsappNumber;
        LastUpdated = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public string WhatsAppNumber { get; private set; } = string.Empty;
    public List<ChatMessage> MessageHistory { get; private set; } = [];
    public DateTimeOffset LastUpdated { get; private set; }

    public static Conversation Create(Guid customerId, string whatsappNumber) => new(customerId, whatsappNumber);

    public void UpdateHistory(List<ChatMessage> messages)
    {
        MessageHistory = messages;
        LastUpdated = DateTimeOffset.UtcNow;
    }
}
