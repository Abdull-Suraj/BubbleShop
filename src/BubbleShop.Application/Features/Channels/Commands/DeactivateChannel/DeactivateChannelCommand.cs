
using BubbleShop.Application.Common.Models;
using MediatR;

namespace BubbleShop.Application.Features.Channels.Commands.DeactivateChannel;

public sealed record DeactivateChannelCommand(
    Guid BusinessId,
    string ChannelType
) : IRequest<Result>;