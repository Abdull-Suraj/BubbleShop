using BubbleShop.Application.Common.Models;
using MediatR;

namespace BubbleShop.Application.Features.Products.Queries.GetAllProducts;

public sealed record GetAllProductsQuery : IRequest<Result<IReadOnlyList<ProductDto>>>;
