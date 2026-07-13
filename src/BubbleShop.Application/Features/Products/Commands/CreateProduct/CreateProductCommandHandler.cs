using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Application.Common.Models;
using BubbleShop.Domain.Entities;
using BubbleShop.Domain.Exceptions;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BubbleShop.Application.Features.Products.Commands.CreateProduct;

public sealed class CreateProductCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork, ILogger<CreateProductCommandHandler> logger)
    : IRequestHandler<CreateProductCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new DomainException("Product name is required.");
        }

        var product = Product.Create(
            request.BusinessId,
            request.Name,
            request.Description,
            request.Price,
            request.StockQuantity,
            request.ImageUrl);
        await productRepository.AddAsync(product, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Created product {ProductId}", product.Id);
        return Result<Guid>.Success(product.Id);
    }
}
