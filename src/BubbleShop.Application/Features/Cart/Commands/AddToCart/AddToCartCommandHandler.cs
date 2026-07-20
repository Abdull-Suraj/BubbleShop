// Application/Features/Cart/Commands/AddToCart/AddToCartCommandHandler.cs
using BubbleShop.Application.AppServices;
using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Application.Common.Models;
using BubbleShop.Domain.Entities;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BubbleShop.Application.Features.Cart.Commands.AddToCart;

public sealed class AddToCartCommandHandler : IRequestHandler<AddToCartCommand, Result<MessageResponse>>
{
    private readonly IProductRepository _productRepository;
    private readonly ICartRepository _cartRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AddToCartCommandHandler> _logger;

    public AddToCartCommandHandler(
        IProductRepository productRepository,
        ICartRepository cartRepository,
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork,
        ILogger<AddToCartCommandHandler> logger)
    {
        _productRepository = productRepository;
        _cartRepository = cartRepository;
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<MessageResponse>> Handle(AddToCartCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var product =
    await _productRepository.GetByIdAsync(
        request.ProductId,
        cancellationToken);
            if (product is null)
            {
                return Result<MessageResponse>.Failure(
                    "Product not found.",
                    "NotFound");
            }
            _logger.LogInformation("Adding {Quantity} of {ProductName} to cart for customer {CustomerId}",
                request.Quantity, product.Name, request.CustomerId);

            if (string.IsNullOrWhiteSpace(product.Name))
            {
                return Result<MessageResponse>.Failure(
                    "Please specify which product you want to add to your cart.",
                    "ValidationError"
                );
            }

            // Get customer
            var customer = await _customerRepository.GetByIdAsync(
       request.CustomerId,
       cancellationToken);

            if (customer is null)
            {
                return Result<MessageResponse>.Failure(
                    "Customer not found. Please register first.",
                    "NotFound"
                );
            }

            // Get product
            

            if (product.Name is null)
            {
                return Result<MessageResponse>.Failure(
                    $"Product '{product.Name}' not found.",
                    "NotFound"
                );
            }

            if (product.StockQuantity < request.Quantity)
            {
                return Result<MessageResponse>.Failure(
                    $"Only {product.StockQuantity} units of {product.Name} available. Would you like to order {product.StockQuantity} instead?",
                    "ValidationError"
                );
            }

            // Get or create cart
            var cart = await _cartRepository.GetOrCreateCartAsync(customer.Id, cancellationToken);

            // Add item to cart
            cart.AddItem(product.Id, product.Name, request.Quantity, product.Price);
            await _cartRepository.UpdateAsync(cart, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var cartTotal = cart.GetTotal();
            var itemCount = cart.GetTotalItems();

            var response = $"✅ **Added to Cart!**\n\n" +
                          $"🛍️ **Product:** {product.Name}\n" +
                          $"🔢 **Quantity:** {request.Quantity}\n" +
                          $"💰 **Subtotal:** {(product.Price * request.Quantity):C}\n" +
                          $"📦 **Cart Total:** {cartTotal:C}\n" +
                          $"🛒 **Items in Cart:** {itemCount}\n\n" +
                          $"What would you like to do next?\n" +
                          $"• `VIEW CART` - See your cart\n" +
                          $"• `CHECKOUT` - Proceed to checkout\n" +
                          $"• `ORDER {product.Name}` - Order now";

            return Result<MessageResponse>.Success(MessageResponse.Success(response));
        }
        catch (Exception ex)
        {
            var product =
await _productRepository.GetByIdAsync(
request.ProductId,
cancellationToken);
            if (product is null)
            {
                return Result<MessageResponse>.Failure(
                    "Product not found.",
                    "NotFound");
            }
            _logger.LogInformation("Adding {Quantity} of {ProductName} to cart for customer {CustomerId}",
                request.Quantity, product.Name, request.CustomerId);
            _logger.LogError(ex, "Error adding {Quantity} of {ProductName} to cart", request.Quantity, product.Name);
            return Result<MessageResponse>.Failure($"Failed to add to cart: {ex.Message}");
        }
    }
}