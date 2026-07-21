// Application/Features/Channels/Commands/DeactivateChannel/DeactivateChannelCommandHandler.cs
using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Application.Common.Models;
using BubbleShop.Domain.Enums;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BubbleShop.Application.Features.Channels.Commands.DeactivateChannel;

public sealed class DeactivateChannelCommandHandler : IRequestHandler<DeactivateChannelCommand, Result>
{
    private readonly IChannelRepository _channelRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeactivateChannelCommandHandler> _logger;

    public DeactivateChannelCommandHandler(
        IChannelRepository channelRepository,
        IUnitOfWork unitOfWork,
        ILogger<DeactivateChannelCommandHandler> logger)
    {
        _channelRepository = channelRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(DeactivateChannelCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Deactivating channel {ChannelType} for business {BusinessId}",
                request.ChannelType, request.BusinessId);

            if (!Enum.TryParse<ChannelType>(request.ChannelType, true, out var channelType))
                return Result.Failure($"Invalid channel type: {request.ChannelType}", "ValidationError");

            var channel = await _channelRepository.GetByBusinessAndTypeAsync(
                request.BusinessId,
                channelType,
                cancellationToken);

            if (channel is null)
                return Result.Failure($"Channel {request.ChannelType} not found", "NotFound");

            channel.Deactivate();
            await _channelRepository.UpdateAsync(channel, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Channel {ChannelType} deactivated successfully", request.ChannelType);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating channel {ChannelType} for business {BusinessId}",
                request.ChannelType, request.BusinessId);
            return Result.Failure($"Failed to deactivate channel: {ex.Message}");
        }
    }
}