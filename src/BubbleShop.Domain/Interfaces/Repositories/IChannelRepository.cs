
using BubbleShop.Domain.Entities;
using BubbleShop.Domain.Enums;

namespace BubbleShop.Domain.Interfaces.Repositories;

public interface IChannelRepository : IRepository<Channel>
{
    Task<Channel?> GetByBusinessAndTypeAsync(Guid businessId, ChannelType channelType, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Channel>> GetByBusinessIdAsync(Guid businessId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Channel>> GetActiveChannelsByBusinessIdAsync(Guid businessId, CancellationToken cancellationToken = default);
    Task<ChannelStatistics> GetChannelStatisticsAsync(Guid channelId, CancellationToken cancellationToken = default);
    Task<Channel?> GetByChannelIdAsync(
    string channelId,
    ChannelType channelType,
    CancellationToken cancellationToken = default);
}
