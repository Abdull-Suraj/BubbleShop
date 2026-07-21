
using BubbleShop.Application.Common.Models;
using MediatR;

namespace BubbleShop.Application.Features.Channels.Commands.ActivateChannel;

public sealed record ActivateChannelCommand(
    Guid BusinessId,
    string ChannelType
) : IRequest<Result>;