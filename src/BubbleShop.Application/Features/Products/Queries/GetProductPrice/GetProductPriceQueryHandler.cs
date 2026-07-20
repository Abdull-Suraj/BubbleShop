
using BubbleShop.Application.AppServices;
using BubbleShop.Application.Common.Models;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BubbleShop.Application.Features.Products.Queries;

public class GetProductPriceQueryHandler : IRequestHandler<GetProductPriceQuery, Result<MessageResponse>>
{
    private readonly IProductRepository _productRepository;

    public GetProductPriceQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<MessageResponse>> Handle(
           GetProductPriceQuery request,
           CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByNameAsync(
            request.ProductName,
            request.BusinessId,
            cancellationToken);

        if (product == null)
        {
            return Result<MessageResponse>.Failure(
                $"Product '{request.ProductName}' not found",
                "NotFound");
        }

        return Result<MessageResponse>.Success(
            MessageResponse.Success(
                $"🛍️ {product.Name}\n" +
                $"💰 Price: {product.Price:C}\n" +
                $"📦 Stock: {product.StockQuantity}"
            ));
    }
}