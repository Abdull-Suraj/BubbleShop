using BubbleShop.Application.Common.Models;
using MediatR;

namespace BubbleShop.Application.Features.Products.Commands.UpdateProduct;

public sealed record UpdateProductCommand(Guid ProductId, string Name, string Description, decimal Price, string? ImageUrl, bool IsActive) : IRequest<Result>;
