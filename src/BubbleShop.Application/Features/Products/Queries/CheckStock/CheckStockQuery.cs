
using BubbleShop.Application.AppServices;
using BubbleShop.Application.Common.Models;
using MediatR;


namespace BubbleShop.Application.Features.Products.Queries;

public sealed record CheckStockQuery(
    Guid BusinessId,
    string ProductName
) : IRequest<Result<MessageResponse>>;