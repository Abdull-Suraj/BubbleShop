
using BubbleShop.Application.Common.Models;
using MediatR;

namespace BubbleShop.Application.Features.Cart.Queries.GetCart;

public sealed record GetCartQuery(
    string Channel,
    Guid CustomerId,
    Guid BusinessId,
    string Message
) : IRequest<Result<AppServices.MessageResponse>>;