
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BubbleShop.Application.Features.Products.Queries;

public class GetProductPriceQueryHandler : IRequestHandler<GetProductPriceQuery, IActionResult>
{
    private readonly IProductRepository _productRepository;

    public GetProductPriceQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<IActionResult> Handle(GetProductPriceQuery request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByNameAsync(request.ProductName, request.BusinessId, cancellationToken);

        if (product == null)
            return new NotFoundObjectResult(new { message = $"Product '{request.ProductName}' not found" });

        return new OkObjectResult(new
        {
            ProductName = product.Name,
            Price = product.Price,
            StockQuantity = product.StockQuantity
        });
    }
}