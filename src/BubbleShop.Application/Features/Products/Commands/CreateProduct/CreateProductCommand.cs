using BubbleShop.Application.Common.Models;
using MediatR;

namespace BubbleShop.Application.Features.Products.Commands.CreateProduct;

public sealed record CreateProductCommand(
    Guid BusinessId,
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    string? ImageUrl,
    string? Category = null)
    : IRequest<Result<Guid>>;
