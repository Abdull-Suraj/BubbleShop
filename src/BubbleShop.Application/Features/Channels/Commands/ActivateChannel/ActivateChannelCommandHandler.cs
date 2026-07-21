using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Application.Common.Models;
using BubbleShop.Application.DTOs;
using BubbleShop.Domain.Enums;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BubbleShop.Application.Features.Channels.Commands.ActivateChannel;

public sealed class ActivateChannelCommandHandler : IRequestHandler<ActivateChannelCommand, Result>
{
    private readonly IChannelRepository _channelRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ActivateChannelCommandHandler> _logger;

    public ActivateChannelCommandHandler(
        IChannelRepository channelRepository,
        IUnitOfWork unitOfWork,
        ILogger<ActivateChannelCommandHandler> logger)
    {
        _channelRepository = channelRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(ActivateChannelCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Activating channel {ChannelType} for business {BusinessId}",
                request.ChannelType, request.BusinessId);

            if (!Enum.TryParse<ChannelType>(request.ChannelType, true, out var channelType))
                return Result<ChannelStatusDto>.Failure($"Invalid channel type: {request.ChannelType}", "ValidationError");

            var channel = await _channelRepository.GetByBusinessAndTypeAsync(
                request.BusinessId,
                channelType,
                cancellationToken);

            if (channel is null)
                return Result.Failure($"Channel {request.ChannelType} not found", "NotFound");

            channel.Activate();
            await _channelRepository.UpdateAsync(channel, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Channel {ChannelType} activated successfully", request.ChannelType);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating channel {ChannelType} for business {BusinessId}",
                request.ChannelType, request.BusinessId);
            return Result.Failure($"Failed to activate channel: {ex.Message}");
        }
    }
}