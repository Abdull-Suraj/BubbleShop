using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Application.Common.Models;
using BubbleShop.Domain.Exceptions;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;

namespace BubbleShop.Application.Features.Products.Commands.UpdateStock;

public sealed class UpdateStockCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateStockCommand, Result>
{
    public async Task<Result> Handle(UpdateStockCommand request, CancellationToken cancellationToken)
    {
        if (request.ProductId == Guid.Empty)
        {
            throw new DomainException("Product ID is required.");
        }

        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
        {
            return Result.Failure("Product not found.");
        }

        product.UpdateStock(request.Quantity);
        await productRepository.UpdateAsync(product, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
