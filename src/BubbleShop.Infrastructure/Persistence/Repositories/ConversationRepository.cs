using BubbleShop.Domain.Entities;
using BubbleShop.Domain.Enums;
using BubbleShop.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BubbleShop.Infrastructure.Persistence.Repositories;

public class ConversationRepository(AppDbContext dbContext) : Repository<Conversation>(dbContext), IConversationRepository
{
    public async Task<Conversation?> GetByWhatsAppNumberAsync(string whatsappNumber, CancellationToken cancellationToken = default)
        => await DbContext.Conversations.FirstOrDefaultAsync(x => x.WhatsAppNumber == whatsappNumber, cancellationToken);
    public async Task<Conversation?> GetByCustomerAndChannelAsync(
    string channelUserId,
    Guid businessId,
    string channel,
    CancellationToken cancellationToken = default)
    {
        return await DbContext.Conversations
            .FirstOrDefaultAsync(
                x =>
                    x.BusinessId == businessId &&
                    x.WhatsAppNumber == channelUserId &&
                    x.Channel == channel &&
                    !x.IsDeleted,
                cancellationToken);
    }
    //public async Task UpdateMessageHistoryAsync(
    //    Guid conversationId,
    //    List<ChatMessage> messages,
    //    CancellationToken cancellationToken = default)
    //{
    //    var conversation = await DbContext.Conversations
    //        .Include(x => x.Messages)
    //        .FirstOrDefaultAsync(x => x.Id == conversationId, cancellationToken);

    //    if (conversation is null)
    //        return;

    //    conversation.Messages.Clear();

    //    foreach (var message in messages)
    //    {
    //        conversation.AddCustomerMessage(
    //            message.Content,
    //            message.Role == ChatRole.User ? "Customer" : "Assistant",
    //            message.Role == ChatRole.User);
    //    }

    //    await DbContext.SaveChangesAsync(cancellationToken);
    //}
}
