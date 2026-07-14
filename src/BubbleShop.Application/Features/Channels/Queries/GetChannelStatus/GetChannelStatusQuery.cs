
using BubbleShop.Application.Common.Models;
using BubbleShop.Application.DTOs;
using MediatR;

namespace BubbleShop.Application.Features.Channels.Queries.GetChannelStatus;

public sealed record GetChannelStatusQuery(
    Guid BusinessId,
    string ChannelType
) : IRequest<Result<ChannelStatusDto>>;