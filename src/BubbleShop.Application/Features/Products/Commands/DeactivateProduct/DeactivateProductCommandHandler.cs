using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Application.Common.Models;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BubbleShop.Application.Features.Products.Commands.DeactivateProduct;

public sealed class DeactivateProductCommandHandler : IRequestHandler<DeactivateProductCommand, Result>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeactivateProductCommandHandler> _logger;

    public DeactivateProductCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        ILogger<DeactivateProductCommandHandler> logger)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(DeactivateProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Deactivating product: {ProductId}", request.ProductId);

            var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
            if (product is null)
                return Result.Failure($"Product {request.ProductId} not found", "NotFound");

            product.Deactivate();
            await _productRepository.UpdateAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating product: {ProductId}", request.ProductId);
            return Result.Failure($"Failed to deactivate product: {ex.Message}");
        }
    }
}