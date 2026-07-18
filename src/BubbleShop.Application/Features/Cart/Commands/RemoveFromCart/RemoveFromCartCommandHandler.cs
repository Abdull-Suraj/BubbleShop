
using BubbleShop.Application.AppServices;
using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Application.Common.Models;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BubbleShop.Application.Features.Cart.Commands.RemoveFromCart;

public sealed class RemoveFromCartCommandHandler : IRequestHandler<RemoveFromCartCommand, Result<MessageResponse>>
{
    private readonly ICartRepository _cartRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RemoveFromCartCommandHandler> _logger;

    public RemoveFromCartCommandHandler(
        ICartRepository cartRepository,
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork,
        ILogger<RemoveFromCartCommandHandler> logger)
    {
        _cartRepository = cartRepository;
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<MessageResponse>> Handle(RemoveFromCartCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Removing {ProductName} from cart for customer {CustomerId}",
                request.ProductName, request.CustomerId);

            if (string.IsNullOrWhiteSpace(request.ProductName))
            {
                return Result<MessageResponse>.Failure(
                    "Please specify which product you want to remove from your cart.",
                    "ValidationError"
                );
            }

            // Get customer
            var customer = await _customerRepository.GetByWhatsAppNumberAsync(
                request.CustomerId,
                request.BusinessId,
                cancellationToken);

            if (customer is null)
            {
                return Result<MessageResponse>.Failure(
                    "Customer not found.",
                    "NotFound"
                );
            }

            // Get cart
            var cart = await _cartRepository.GetByCustomerIdAsync(customer.Id, cancellationToken);

            if (cart is null || !cart.Items.Any())
            {
                return Result<MessageResponse>.Failure(
                    "Your cart is already empty! 🛒",
                    "ValidationError"
                );
            }

            // Remove item
            var removed = cart.RemoveItem(request.ProductId);

            if (!removed)
            {
                return Result<MessageResponse>.Failure(
                    $"'{request.ProductName}' not found in your cart.",
                    "NotFound"
                );
            }

            await _cartRepository.UpdateAsync(cart, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var cartTotal = cart.GetTotal();
            var itemCount = cart.GetTotalItems();

            if (itemCount == 0)
            {
                return Result<MessageResponse>.Success(new MessageResponse(
                    $"✅ **Cart Updated**\n\n" +
                    $"Removed '{request.ProductName}' from your cart.\n" +
                    $"Your cart is now empty. 🛒\n\n" +
                    $"Reply `MENU` to browse products."
                ));
            }

            var response = $"✅ **Cart Updated**\n\n" +
                          $"Removed '{request.ProductName}' from your cart.\n" +
                          $"📦 **Cart Total:** {cartTotal:C}\n" +
                          $"🛒 **Items in Cart:** {itemCount}\n\n" +
                          $"Reply `VIEW CART` to see your cart.";

            return Result<MessageResponse>.Success(new MessageResponse(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing {ProductName} from cart", request.ProductName);
            return Result<MessageResponse>.Failure($"Failed to remove from cart: {ex.Message}");
        }
    }
}