
using BubbleShop.Application.AppServices;
using BubbleShop.Application.Common.Models;
using MediatR;

namespace BubbleShop.Application.Features.Products.Commands.CheckStock;

public sealed record CheckStockCommand(
    string Channel,
    string CustomerId,
    Guid BusinessId,
    string ProductName,
    string Message
) : IRequest<Result<MessageResponse>>;