
using BubbleShop.Application.AppServices;
using BubbleShop.Application.Common.Models;
using MediatR;

namespace BubbleShop.Application.Features.Orders.Commands.TrackOrder;

public sealed record TrackOrderCommand(
    string Channel,
    string CustomerId,
    Guid BusinessId,
    string OrderNumber,
    string Message
) : IRequest<Result<MessageResponse>>;