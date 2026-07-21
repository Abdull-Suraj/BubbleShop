
using BubbleShop.Application.AppServices;
using BubbleShop.Application.Common.Models;
using MediatR;


namespace BubbleShop.Application.Features.Products.Queries;

public sealed record GetProductPriceQuery(
    Guid BusinessId,
    string ProductName
) : IRequest<Result<MessageResponse>>;