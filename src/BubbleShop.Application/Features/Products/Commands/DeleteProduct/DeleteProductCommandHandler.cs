using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Application.Common.Models;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BubbleShop.Application.Features.Products.Commands.DeleteProduct;

public sealed class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Result>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteProductCommandHandler> _logger;

    public DeleteProductCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        ILogger<DeleteProductCommandHandler> logger)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Deleting product: {ProductId}", request.ProductId);

            var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
            if (product is null)
                return Result.Failure($"Product {request.ProductId} not found", "NotFound");

            // Check if product has any orders
            if (product.OrderItems.Any())
                return Result.Failure("Cannot delete product with existing orders. Consider deactivating instead.", "ValidationError");

            await _productRepository.DeleteAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Product deleted successfully: {ProductId}", request.ProductId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product: {ProductId}", request.ProductId);
            return Result.Failure($"Failed to delete product: {ex.Message}");
        }
    }
}