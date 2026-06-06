using BubbleShop.Application.Common.Models;
using MediatR;

namespace BubbleShop.Application.Features.Products.Commands.CreateProduct;

public sealed record CreateProductCommand(string Name, string Description, decimal Price, int StockQuantity, string? ImageUrl) : IRequest<Result<Guid>>;
