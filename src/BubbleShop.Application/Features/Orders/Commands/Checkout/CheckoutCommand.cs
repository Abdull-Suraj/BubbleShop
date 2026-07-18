using BubbleShop.Application.AppServices;
using BubbleShop.Application.Common.Models;
using MediatR;

namespace BubbleShop.Application.Features.Orders.Commands.Checkout;

public sealed record CheckoutCommand(
    Guid CustomerId,
    Guid BusinessId,
    string Channel = "WhatsApp"
) : IRequest<Result<MessageResponse>>;