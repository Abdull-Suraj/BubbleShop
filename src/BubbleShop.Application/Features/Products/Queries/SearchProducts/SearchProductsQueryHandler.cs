using AutoMapper;
using BubbleShop.Application.Common.Models;
using BubbleShop.Application.DTOs;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BubbleShop.Application.Features.Products.Queries.SearchProducts;

public sealed class SearchProductsQueryHandler : IRequestHandler<SearchProductsQuery, Result<PagedResult<ProductDto>>>
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<SearchProductsQueryHandler> _logger;

    public SearchProductsQueryHandler(
        IProductRepository productRepository,
        IMapper mapper,
        ILogger<SearchProductsQueryHandler> logger)
    {
        _productRepository = productRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<PagedResult<ProductDto>>> Handle(SearchProductsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
     "Searching products with keyword: {Keyword}",
     request.Keyword);

            // Get all products (or search with keyword and category)
            var allProducts = await _productRepository.GetAllAsync(cancellationToken);
            var products = allProducts.ToList();

            // Apply keyword filter
            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var keyword = request.Keyword.ToLower();
                products = products.Where(p =>
                    p.Name.ToLower().Contains(keyword) ||
                    p.Description.ToLower().Contains(keyword) ||
                    p.SKU.ToLower().Contains(keyword)
                ).ToList();
            }



            // Apply business filter
            if (request.BusinessId.HasValue)
            {
                products = products.Where(p => p.BusinessId == request.BusinessId.Value).ToList();
            }

            // Apply price filters
            if (request.MinPrice.HasValue)
            {
                products = products.Where(p => p.Price >= request.MinPrice.Value).ToList();
            }
            if (request.MaxPrice.HasValue)
            {
                products = products.Where(p => p.Price <= request.MaxPrice.Value).ToList();
            }

            // Get total count
            var totalCount = products.Count;

            // Apply sorting
            products = request.SortBy?.ToLower() switch
            {
                "price" => request.SortDesc ? products.OrderByDescending(p => p.Price).ToList() : products.OrderBy(p => p.Price).ToList(),
                "name" => request.SortDesc ? products.OrderByDescending(p => p.Name).ToList() : products.OrderBy(p => p.Name).ToList(),
                "createdat" => request.SortDesc ? products.OrderByDescending(p => p.CreatedAt).ToList() : products.OrderBy(p => p.CreatedAt).ToList(),
                _ => products.OrderBy(p => p.Name).ToList()
            };

            // Apply pagination
            var paginatedProducts = products
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var productDtos = _mapper.Map<IReadOnlyList<ProductDto>>(paginatedProducts);

            var result = new PagedResult<ProductDto>
            {
                Items = productDtos,
                TotalCount = totalCount,
                Page = request.PageNumber,
                PageSize = request.PageSize
            };

            return Result<PagedResult<ProductDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching products");
            return Result<PagedResult<ProductDto>>.Failure($"Failed to search products: {ex.Message}");
        }
    }
}