using BubbleShop.Application.Common.Models;
using BubbleShop.Application.DTOs;
using BubbleShop.Domain.Enums;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BubbleShop.Application.Features.Channels.Queries.GetChannelStatus;

public sealed class GetChannelStatusQueryHandler : IRequestHandler<GetChannelStatusQuery, Result<ChannelStatusDto>>
{
    private readonly IChannelRepository _channelRepository;
    private readonly IBusinessRepository _businessRepository;
    private readonly ILogger<GetChannelStatusQueryHandler> _logger;

    public GetChannelStatusQueryHandler(
        IChannelRepository channelRepository,
        IBusinessRepository businessRepository,
        ILogger<GetChannelStatusQueryHandler> logger)
    {
        _channelRepository = channelRepository;
        _businessRepository = businessRepository;
        _logger = logger;
    }

    public async Task<Result<ChannelStatusDto>> Handle(GetChannelStatusQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting status for channel {ChannelType} of business {BusinessId}",
                request.ChannelType, request.BusinessId);

            // Validate business exists
            var business = await _businessRepository.GetByIdAsync(request.BusinessId, cancellationToken);
            if (business is null)
                return Result<ChannelStatusDto>.Failure($"Business {request.BusinessId} not found", "NotFound");

            // Validate channel type
            if (!Enum.TryParse<ChannelType>(request.ChannelType, true, out var channelType))
                return Result<ChannelStatusDto>.Failure($"Invalid channel type: {request.ChannelType}", "ValidationError");

            // Get channel
            var channel = await _channelRepository.GetByBusinessAndTypeAsync(
                request.BusinessId,
                channelType,
                cancellationToken);

            if (channel is null)
                return Result<ChannelStatusDto>.Failure($"Channel {request.ChannelType} not found for this business", "NotFound");

            // Get channel statistics
            var stats = await _channelRepository.GetChannelStatisticsAsync(channel.Id, cancellationToken);

            var statusDto = new ChannelStatusDto
            {
                Id = channel.Id,
                BusinessId = channel.BusinessId,
                BusinessName = business.BusinessName,
                ChannelType = channel.ChannelType.ToString(),
                IsActive = channel.IsActive,
                IsVerified = channel.IsVerified,
                WebhookUrl = channel.WebhookUrl,
                CreatedAt = channel.CreatedAt,
                LastActiveAt = channel.LastActiveAt,
                TotalMessagesReceived = stats.TotalMessagesReceived,
                TotalMessagesSent = stats.TotalMessagesSent,
                TotalConversations = stats.TotalConversations,
                AverageResponseTime = stats.AverageResponseTime,
                Status = channel.IsActive ? "Online" : "Offline"
            };

            return Result<ChannelStatusDto>.Success(statusDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting channel status for {ChannelType} of business {BusinessId}",
                request.ChannelType, request.BusinessId);
            return Result<ChannelStatusDto>.Failure($"Failed to get channel status: {ex.Message}");
        }
    }
}