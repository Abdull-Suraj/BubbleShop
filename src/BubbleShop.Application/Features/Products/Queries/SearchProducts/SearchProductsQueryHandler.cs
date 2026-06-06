using AutoMapper;
using BubbleShop.Application.Common.Models;
using BubbleShop.Application.Features.Products.Queries.GetAllProducts;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BubbleShop.Application.Features.Products.Queries.SearchProducts;

public sealed class SearchProductsQueryHandler(IProductRepository productRepository, IMapper mapper, ILogger<SearchProductsQueryHandler> logger)
    : IRequestHandler<SearchProductsQuery, Result<IReadOnlyList<ProductDto>>>
{
    public async Task<Result<IReadOnlyList<ProductDto>>> Handle(SearchProductsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Searching products with keyword {Keyword}", request.Keyword);
        var products = await productRepository.SearchAsync(request.Keyword, request.Category, cancellationToken);
        return Result<IReadOnlyList<ProductDto>>.Success(mapper.Map<IReadOnlyList<ProductDto>>(products));
    }
}
