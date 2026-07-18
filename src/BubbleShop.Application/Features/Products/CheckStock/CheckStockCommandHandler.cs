// Application/Features/Products/Commands/CheckStock/CheckStockCommandHandler.cs
using BubbleShop.Application.AppServices;
using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Application.Common.Models;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BubbleShop.Application.Features.Products.Commands.CheckStock;

public sealed class CheckStockCommandHandler : IRequestHandler<CheckStockCommand, Result<MessageResponse>>
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<CheckStockCommandHandler> _logger;

    public CheckStockCommandHandler(
        IProductRepository productRepository,
        ILogger<CheckStockCommandHandler> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<Result<MessageResponse>> Handle(CheckStockCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Checking stock for product {ProductName}", request.ProductName);

            if (string.IsNullOrWhiteSpace(request.ProductName))
            {
                return Result<MessageResponse>.Failure(
                    "Please specify which product you want to check stock for.",
                    "ValidationError"
                );
            }

            var product = await _productRepository.GetByNameAsync(request.ProductName, request.BusinessId, cancellationToken);

            if (product is null)
            {
                // Try partial match
                var products = await _productRepository.SearchAsync(request.ProductName, null, cancellationToken);
                if (products.Any())
                {
                    var suggestions = string.Join("\n• ", products.Take(5).Select(p => p.Name));
                    return Result<MessageResponse>.Failure(
                        $"Couldn't find '{request.ProductName}'. Did you mean:\n• {suggestions}",
                        "NotFound"
                    );
                }

                return Result<MessageResponse>.Failure(
                    $"Sorry, we don't have '{request.ProductName}' in our store.",
                    "NotFound"
                );
            }

            var stockStatus = product.StockQuantity > 10 ? "✅ In Stock" :
                              product.StockQuantity > 0 ? "⚠️ Low Stock" : "❌ Out of Stock";

            var emoji = product.StockQuantity > 10 ? "📦" :
                        product.StockQuantity > 0 ? "⚠️" : "❌";

            var response = $"{emoji} **Stock Check**\n\n" +
                          $"🛍️ **Product:** {product.Name}\n" +
                          $"📊 **Status:** {stockStatus}\n" +
                          $"🔢 **Quantity Available:** {product.StockQuantity} units\n";

            if (product.StockQuantity <= 10 && product.StockQuantity > 0)
            {
                response += $"\n⚠️ **Low Stock Alert!** Only {product.StockQuantity} units left.\n";
                response += $"Order soon to avoid disappointment! 🏃‍♂️\n";
            }

            if (product.StockQuantity == 0)
            {
                response += $"\n❌ **Out of Stock!**\n";
                response += $"Would you like to be notified when it's back in stock?\n";
                response += $"Reply `NOTIFY ME` to get an update.";
            }
            else
            {
                response += $"\n💰 **Price:** {product.Price:C}\n";
                response += $"\nWould you like to order this? Reply `ORDER {product.Name}`.";
            }

            return Result<MessageResponse>.Success(new MessageResponse(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking stock for product {ProductName}", request.ProductName);
            return Result<MessageResponse>.Failure($"Failed to check stock: {ex.Message}");
        }
    }
}