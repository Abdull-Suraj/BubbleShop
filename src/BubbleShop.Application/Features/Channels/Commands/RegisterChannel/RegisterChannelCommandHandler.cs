// Application/Features/Channels/Commands/RegisterChannel/RegisterChannelCommandHandler.cs
using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Application.Common.Models;
using BubbleShop.Domain.Entities;
using BubbleShop.Domain.Enums;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;


namespace BubbleShop.Application.Features.Channels.Commands.RegisterChannel;

public sealed class RegisterChannelCommandHandler : IRequestHandler<RegisterChannelCommand, Result<Guid>>
{
    private readonly IChannelRepository _channelRepository;
    private readonly IBusinessRepository _businessRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RegisterChannelCommandHandler> _logger;

    public RegisterChannelCommandHandler(
        IChannelRepository channelRepository,
        IBusinessRepository businessRepository,
        IUnitOfWork unitOfWork,
        ILogger<RegisterChannelCommandHandler> logger)
    {
        _channelRepository = channelRepository;
        _businessRepository = businessRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(RegisterChannelCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Registering channel {ChannelType} for business {BusinessId}",
                request.ChannelType, request.BusinessId);

            // Validate business exists
            var business = await _businessRepository.GetByIdAsync(request.BusinessId, cancellationToken);
            if (business is null)
                return Result<Guid>.Failure($"Business {request.BusinessId} not found", "NotFound");

            // Validate channel type
            if (!Enum.TryParse<ChannelType>(request.ChannelType, true, out var channelType))
                return Result<Guid>.Failure($"Invalid channel type: {request.ChannelType}", "ValidationError");

            // Check if channel already exists for this business
            var existingChannel = await _channelRepository.GetByBusinessAndTypeAsync(
                request.BusinessId,
                channelType,
                cancellationToken);

            if (existingChannel is not null)
                return Result<Guid>.Failure($"Channel {request.ChannelType} already registered for this business", "ValidationError");

            // Create new channel
            var channel = new Channel(
                businessId: request.BusinessId,
                channelType: channelType,
                webhookUrl: request.WebhookUrl,
                apiKey: request.ApiKey,
                isActive: request.IsActive
            );

            await _channelRepository.AddAsync(channel, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Channel {ChannelType} registered successfully with ID: {ChannelId}",
                request.ChannelType, channel.Id);

            return Result<Guid>.Success(channel.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering channel {ChannelType} for business {BusinessId}",
                request.ChannelType, request.BusinessId);
            return Result<Guid>.Failure($"Failed to register channel: {ex.Message}");
        }
    }
}