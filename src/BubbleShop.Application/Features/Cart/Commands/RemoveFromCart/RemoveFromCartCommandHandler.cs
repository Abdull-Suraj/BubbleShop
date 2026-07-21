
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

    public async Task<Result<MessageResponse>> Handle(
       RemoveFromCartCommand request,
       CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Removing cart item {CartItemId} for customer {CustomerId}",
                request.CartItemId,
                request.CustomerId);


            var customer = await _customerRepository.GetByIdAsync(
                request.CustomerId,
                cancellationToken);


            if (customer is null)
            {
                return Result<MessageResponse>.Failure(
                    "Customer not found.",
                    "NotFound");
            }


            var cart = await _cartRepository.GetByCustomerIdAsync(
                customer.Id,
                cancellationToken);


            if (cart is null || !cart.Items.Any())
            {
                return Result<MessageResponse>.Failure(
                    "Your cart is empty 🛒",
                    "ValidationError");
            }


            var removed = cart.RemoveItem(request.CartItemId);


            if (!removed)
            {
                return Result<MessageResponse>.Failure(
                    "Item was not found in your cart.",
                    "NotFound");
            }


            await _unitOfWork.SaveChangesAsync(cancellationToken);


            var response =
                $"✅ **Cart Updated**\n\n" +
                $"Item removed successfully.\n" +
                $"🛒 Remaining Items: {cart.GetTotalItems()}\n" +
                $"💰 Total: {cart.GetTotal():C}";


            return Result<MessageResponse>.Success(
                MessageResponse.Success(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error removing cart item {CartItemId}",
                request.CartItemId);

            return Result<MessageResponse>.Failure(
                $"Failed to remove item: {ex.Message}");
        }
    }
}