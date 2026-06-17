using AutoMapper;
using BubbleShop.Application.Common.Models;
using BubbleShop.Application.DTOs;
using BubbleShop.Application.Features.Products.Queries.GetAllProducts;
using BubbleShop.Domain.Exceptions;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BubbleShop.Application.Features.Products.Queries.GetProductById;

public sealed class GetProductByIdQueryHandler(IProductRepository productRepository, IMapper mapper, ILogger<GetProductByIdQueryHandler> logger)
    : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
{
    public async Task<Result<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        if (request.ProductId == Guid.Empty)
        {
            throw new DomainException("Product ID is required.");
        }

        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
        {
            logger.LogWarning("Product not found: {ProductId}", request.ProductId);
            return Result<ProductDto>.Failure("Product not found.");
        }

        return Result<ProductDto>.Success(mapper.Map<ProductDto>(product));
    }
}
