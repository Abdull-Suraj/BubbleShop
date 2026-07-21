// Application/Features/Channels/Queries/GetBusinessChannels/GetBusinessChannelsQueryHandler.cs
using BubbleShop.Application.Common.Models;
using BubbleShop.Application.DTOs;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BubbleShop.Application.Features.Channels.Queries.GetBusinessChannels;

public sealed class GetBusinessChannelsQueryHandler : IRequestHandler<GetBusinessChannelsQuery, Result<IReadOnlyList<ChannelDto>>>
{
    private readonly IChannelRepository _channelRepository;
    private readonly ILogger<GetBusinessChannelsQueryHandler> _logger;

    public GetBusinessChannelsQueryHandler(
        IChannelRepository channelRepository,
        ILogger<GetBusinessChannelsQueryHandler> logger)
    {
        _channelRepository = channelRepository;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<ChannelDto>>> Handle(GetBusinessChannelsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting all channels for business {BusinessId}", request.BusinessId);

            var channels = await _channelRepository.GetByBusinessIdAsync(request.BusinessId, cancellationToken);

            var channelDtos = channels.Select(c => new ChannelDto
            {
                Id = c.Id,
                BusinessId = c.BusinessId,
                ChannelType = c.ChannelType.ToString(),
                IsActive = c.IsActive,
                IsVerified = c.IsVerified,
                WebhookUrl = c.WebhookUrl,
                CreatedAt = c.CreatedAt,
                LastActiveAt = c.LastActiveAt
            }).ToList();

            return Result<IReadOnlyList<ChannelDto>>.Success(channelDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting channels for business {BusinessId}", request.BusinessId);
            return Result<IReadOnlyList<ChannelDto>>.Failure($"Failed to get channels: {ex.Message}");
        }
    }
}