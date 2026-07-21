
using Microsoft.EntityFrameworkCore;
using BubbleShop.Domain.Entities;
using BubbleShop.Domain.Enums;
using BubbleShop.Domain.Interfaces.Repositories;
using BubbleShop.Infrastructure.Persistence;

namespace BubbleShop.Infrastructure.Persistence.Repositories;

public class ChannelRepository : Repository<Channel>, IChannelRepository
{
    public ChannelRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Channel?> GetByBusinessAndTypeAsync(Guid businessId, ChannelType channelType, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.BusinessId == businessId
                                   && c.ChannelType == channelType
                                   && !c.IsDeleted, cancellationToken);
    }
    public async Task<Channel?> GetByChannelIdAsync(
    string channelId,
    ChannelType channelType,
    CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(
                c => c.ChannelId == channelId
                     && c.ChannelType == channelType
                     && !c.IsDeleted,
                cancellationToken);
    }
    public async Task<IReadOnlyList<Channel>> GetByBusinessIdAsync(Guid businessId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.BusinessId == businessId && !c.IsDeleted)
            .OrderBy(c => c.ChannelType)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Channel>> GetActiveChannelsByBusinessIdAsync(Guid businessId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.BusinessId == businessId && c.IsActive && !c.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<ChannelStatistics> GetChannelStatisticsAsync(Guid channelId, CancellationToken cancellationToken = default)
    {
        // This would typically query related tables (messages, conversations, etc.)
        // For now, return default statistics
        return new ChannelStatistics
        {
            TotalMessagesReceived = 0,
            TotalMessagesSent = 0,
            TotalConversations = 0,
            AverageResponseTime = 0
        };
    }
}