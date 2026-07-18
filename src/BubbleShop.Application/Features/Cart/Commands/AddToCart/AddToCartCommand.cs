using BubbleShop.Application.AppServices;
using BubbleShop.Application.Common.Models;
using MediatR;

namespace BubbleShop.Application.Features.Cart.Commands.AddToCart;

public sealed record AddToCartCommand(
    string Channel,
    Guid CustomerId,
    Guid BusinessId,
    Guid ProductId,
    int Quantity,
    string Message
) : IRequest<Result<MessageResponse>>;