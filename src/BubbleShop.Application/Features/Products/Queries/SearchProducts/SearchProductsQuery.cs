
using BubbleShop.Application.Common.Models;
using BubbleShop.Application.DTOs;
using BubbleShop.Application.Features.Products.Queries.GetAllProducts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BubbleShop.Application.Features.Products.Queries.SearchProducts;

public sealed record SearchProductsQuery(
    string? Keyword = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    int PageNumber = 1,
    int PageSize = 20,
    Guid? BusinessId = null,
    string? SortBy = "name",
    bool SortDesc = false
) : IRequest<Result<PagedResult<ProductDto>>>;
//IRequest<Result<PagedResult<DTOs.ProductDto>>>;