
using BubbleShop.Application.Common.Models;
using MediatR;

namespace BubbleShop.Application.Features.Channels.Commands.RegisterChannel;

public sealed record RegisterChannelCommand(
    Guid BusinessId,
    string ChannelType,
    string? WebhookUrl = null,
    string? ApiKey = null,
    bool IsActive = true
) : IRequest<Result<Guid>>;