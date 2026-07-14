// Application/Services/ChannelFactory.cs
using BubbleShop.Application.Channels;
using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Domain.Enums;
using BubbleShop.Domain.Interfaces.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BubbleShop.Application.AppServices;

public class ChannelFactory : IChannelFactory
{

    private readonly IChannelRepository _channelRepository;
    private readonly ILogger<ChannelFactory> _logger;
    private readonly Dictionary<ChannelType, IChannelAdapter> _channelAdapters;

//private readonly Dictionary<ChannelType, IChannelAdapter> _channelAdapters;

public ChannelFactory(
    IEnumerable<IChannelAdapter> adapters,
    IChannelRepository channelRepository,
    ILogger<ChannelFactory> logger)
{
    _channelRepository = channelRepository;
    _logger = logger;

    _channelAdapters = adapters
        .GroupBy(x => x.ChannelType)
        .ToDictionary(x => x.Key, x => x.First());

    foreach (var adapter in _channelAdapters)
    {
        _logger.LogInformation(
            "Registered channel adapter: {ChannelType}",
            adapter.Key);
    }
}


    public IChannelAdapter GetChannelAdapter(ChannelType channelType)
    {
        if (_channelAdapters.TryGetValue(channelType, out var adapter))
        {
            return adapter;
        }

        throw new ArgumentException(
            $"No adapter registered for channel type: {channelType}");
    }
    public IReadOnlyList<IChannelAdapter> GetAllChannelAdapters()
    {
        return _channelAdapters.Values.ToList().AsReadOnly();
    }

    public async Task<bool> RegisterChannelAsync(Guid businessId, ChannelType channelType, Dictionary<string, string> config, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Registering channel {ChannelType} for business {BusinessId}", channelType, businessId);

            // Get the adapter
            var adapter = GetChannelAdapter(channelType);

            // Start listening
            await adapter.StartListeningAsync(cancellationToken);

            // Save channel configuration to database
            // This would be handled by your Channel repository

            _logger.LogInformation("Channel {ChannelType} registered successfully for business {BusinessId}", channelType, businessId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register channel {ChannelType} for business {BusinessId}", channelType, businessId);
            return false;
        }
    }

    public bool IsChannelAvailable(ChannelType channelType)
    {
        var available = _channelAdapters.ContainsKey(channelType);

        if (available)
        {
            _logger.LogInformation(
                "Channel {ChannelType} is available",
                channelType);
        }

        return available;
    }

    public async Task<Dictionary<string, string>> GetChannelConfigAsync(Guid businessId, ChannelType channelType, CancellationToken cancellationToken = default)
    {
        try
        {
            var channel = await _channelRepository.GetByBusinessAndTypeAsync(businessId, channelType, cancellationToken);

            if (channel is null)
                return new Dictionary<string, string>();

            return new Dictionary<string, string>
            {
                ["webhookUrl"] = channel.WebhookUrl ?? string.Empty,
                ["apiKey"] = channel.ApiKey ?? string.Empty,
                ["isActive"] = channel.IsActive.ToString(),
                ["isVerified"] = channel.IsVerified.ToString(),
                ["createdAt"] = channel.CreatedAt.ToString("o"),
                ["lastActiveAt"] = channel.LastActiveAt?.ToString("o") ?? string.Empty
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get channel config for {ChannelType} of business {BusinessId}", channelType, businessId);
            return new Dictionary<string, string>();
        }
    }
}