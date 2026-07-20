// Application/Features/Orders/Commands/CancelOrder/CancelOrderCommand.cs
using BubbleShop.Application.AppServices;
using BubbleShop.Application.Common.Models;
using MediatR;

namespace BubbleShop.Application.Features.Orders.Commands.CancelOrder;

public sealed record CancelOrderCommand(
    Guid OrderId,
    Guid BusinessId,
    string ChannelUserId,
    string Channel,
    string? Reason
) : IRequest<Result<bool>>;