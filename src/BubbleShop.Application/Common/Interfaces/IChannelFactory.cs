
using BubbleShop.Domain.Enums;

namespace BubbleShop.Application.Common.Interfaces;

public interface IChannelFactory
{

    IChannelAdapter GetChannelAdapter(ChannelType channelType);

    IReadOnlyList<IChannelAdapter> GetAllChannelAdapters();


    Task<bool> RegisterChannelAsync(Guid businessId, ChannelType channelType, Dictionary<string, string> config, CancellationToken cancellationToken = default);

 
    bool IsChannelAvailable(ChannelType channelType);

    Task<Dictionary<string, string>> GetChannelConfigAsync(Guid businessId, ChannelType channelType, CancellationToken cancellationToken = default);
}