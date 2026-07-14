using BubbleShop.Domain.Entities;

namespace BubbleShop.Domain.Interfaces.Repositories;

public interface IConversationRepository : IRepository<Conversation>
{
    Task<Conversation?> GetByWhatsAppNumberAsync(string whatsappNumber, CancellationToken cancellationToken = default);
    //Task UpdateMessageHistoryAsync(Guid conversationId, List<ChatMessage> messages, CancellationToken cancellationToken = default);
    Task<Conversation?> GetByCustomerAndChannelAsync(
    string channelUserId,
    Guid businessId,
    string channel,
    CancellationToken cancellationToken = default);
}
