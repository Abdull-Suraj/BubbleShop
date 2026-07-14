// Application/Features/Orders/Commands/CancelOrder/CancelOrderCommand.cs
using BubbleShop.Application.AppServices;
using BubbleShop.Application.Common.Models;
using MediatR;

namespace BubbleShop.Application.Features.Orders.Commands.CancelOrder;

public sealed record CancelOrderCommand(
    string Channel,
    string CustomerId,
    Guid BusinessId,
    Guid OrderId,
    string Reason,
    string Message
) : IRequest<Result<MessageResponse>>;