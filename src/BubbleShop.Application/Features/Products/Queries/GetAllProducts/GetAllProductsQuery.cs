// Application/Features/Products/Queries/GetAllProducts/GetAllProductsQuery.cs
using BubbleShop.Application.Common.Models;
using BubbleShop.Application.DTOs;
using MediatR;

namespace BubbleShop.Application.Features.Products.Queries.GetAllProducts;

// Make all parameters optional with default values
public sealed record GetAllProductsQuery(
    int PageNumber = 1,
    int PageSize = 20,
    Guid? BusinessId = null,
    bool? IsActive = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null
) : IRequest<Result<IReadOnlyList<ProductDto>>>;