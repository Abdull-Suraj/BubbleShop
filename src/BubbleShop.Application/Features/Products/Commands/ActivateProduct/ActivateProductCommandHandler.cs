using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Application.Common.Models;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BubbleShop.Application.Features.Products.Commands.ActivateProduct;

public sealed class ActivateProductCommandHandler : IRequestHandler<ActivateProductCommand, Result>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ActivateProductCommandHandler> _logger;

    public ActivateProductCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        ILogger<ActivateProductCommandHandler> logger)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(ActivateProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Activating product: {ProductId}", request.ProductId);

            var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
            if (product is null)
                return Result.Failure($"Product {request.ProductId} not found", "NotFound");

            product.Activate();
            await _productRepository.UpdateAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating product: {ProductId}", request.ProductId);
            return Result.Failure($"Failed to activate product: {ex.Message}");
        }
    }
}