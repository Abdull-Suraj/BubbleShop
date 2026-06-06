using BubbleShop.Application.Common.Models;
using BubbleShop.Application.Features.Products.Queries.GetAllProducts;
using MediatR;

namespace BubbleShop.Application.Features.Products.Queries.SearchProducts;

public sealed record SearchProductsQuery(string? Keyword, string? Category) : IRequest<Result<IReadOnlyList<ProductDto>>>;
