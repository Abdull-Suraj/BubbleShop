using BubbleShop.Application.Common.Models;
using BubbleShop.Application.Features.Products.Queries.GetAllProducts;
using MediatR;

namespace BubbleShop.Application.Features.Products.Queries.GetProductById;

public sealed record GetProductByIdQuery(Guid ProductId) : IRequest<Result<ProductDto>>;
