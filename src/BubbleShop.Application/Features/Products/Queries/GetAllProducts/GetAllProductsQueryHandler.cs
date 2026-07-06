
using BubbleShop.Application.Common.Mappings;
using BubbleShop.Application.Common.Models;
using BubbleShop.Application.DTOs;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BubbleShop.Application.Features.Products.Queries.GetAllProducts;

public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, Result<IReadOnlyList<ProductDto>>>
{
    private readonly IProductRepository _productRepository;

    private readonly ILogger<GetAllProductsQueryHandler> _logger;

    public GetAllProductsQueryHandler(
        IProductRepository productRepository,

        ILogger<GetAllProductsQueryHandler> logger)
    {
        _productRepository = productRepository;
  
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<ProductDto>>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Fetching all products - Page: {Page}, PageSize: {PageSize}",
                request.PageNumber, request.PageSize);

            var allProducts = await _productRepository.GetAllAsync(cancellationToken);
            var products = allProducts.ToList();

            // Apply filters
            if (request.BusinessId.HasValue)
            {
                products = products.Where(p => p.BusinessId == request.BusinessId.Value).ToList();
            }


            if (request.IsActive.HasValue)
            {
                products = products.Where(p => p.IsActive == request.IsActive.Value).ToList();
            }

            if (request.MinPrice.HasValue)
            {
                products = products.Where(p => p.Price >= request.MinPrice.Value).ToList();
            }

            if (request.MaxPrice.HasValue)
            {
                products = products.Where(p => p.Price <= request.MaxPrice.Value).ToList();
            }

            // Apply pagination
            var paginatedProducts = products
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var productDtos = ProductMapper.ToDtoList(paginatedProducts);

            return Result<IReadOnlyList<ProductDto>>.Success(productDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all products");
            return Result<IReadOnlyList<ProductDto>>.Failure($"Failed to fetch products: {ex.Message}");
        }
    }
}