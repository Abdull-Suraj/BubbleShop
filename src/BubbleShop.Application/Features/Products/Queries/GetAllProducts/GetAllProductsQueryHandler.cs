using AutoMapper;
using BubbleShop.Application.Common.Models;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BubbleShop.Application.Features.Products.Queries.GetAllProducts;

public sealed class GetAllProductsQueryHandler(IProductRepository productRepository, IMapper mapper, ILogger<GetAllProductsQueryHandler> logger)
    : IRequestHandler<GetAllProductsQuery, Result<IReadOnlyList<ProductDto>>>
{
    public async Task<Result<IReadOnlyList<ProductDto>>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching all products");
        var products = await productRepository.GetAllAsync(cancellationToken);
        return Result<IReadOnlyList<ProductDto>>.Success(mapper.Map<IReadOnlyList<ProductDto>>(products));
    }
}
