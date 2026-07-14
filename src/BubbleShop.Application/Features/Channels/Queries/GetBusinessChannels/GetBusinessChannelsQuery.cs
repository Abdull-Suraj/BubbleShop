// Application/Features/Channels/Queries/GetBusinessChannels/GetBusinessChannelsQuery.cs
using BubbleShop.Application.Common.Models;
using BubbleShop.Application.DTOs;
using MediatR;

namespace BubbleShop.Application.Features.Channels.Queries.GetBusinessChannels;

public sealed record GetBusinessChannelsQuery(Guid BusinessId) : IRequest<Result<IReadOnlyList<ChannelDto>>>;