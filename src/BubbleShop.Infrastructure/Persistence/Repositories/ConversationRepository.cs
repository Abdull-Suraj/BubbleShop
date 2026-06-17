using BubbleShop.Domain.Entities;
using BubbleShop.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BubbleShop.Infrastructure.Persistence.Repositories;

public class ConversationRepository(AppDbContext dbContext) : Repository<Conversation>(dbContext), IConversationRepository
{
    public async Task<Conversation?> GetByWhatsAppNumberAsync(string whatsappNumber, CancellationToken cancellationToken = default)
        => await DbContext.Conversations.FirstOrDefaultAsync(x => x.WhatsAppNumber == whatsappNumber, cancellationToken);

    public async Task UpdateMessageHistoryAsync(Guid conversationId, List<ChatMessage> messages, CancellationToken cancellationToken = default)
    {
        var conversation = await DbContext.Conversations.FirstOrDefaultAsync(x => x.Id == conversationId, cancellationToken);
        if (conversation is null)
        {
            return;
        }

        conversation.UpdateHistory(messages);
        DbContext.Conversations.Update(conversation);
    }
}
