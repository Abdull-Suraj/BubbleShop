// Application/Features/Orders/Commands/Checkout/CheckoutCommandHandler.cs
using BubbleShop.Application.AppServices;
using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Application.Common.Models;
using BubbleShop.Domain.Enums;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BubbleShop.Application.Features.Orders.Commands.Checkout;

public sealed class CheckoutCommandHandler : IRequestHandler<CheckoutCommand, Result<MessageResponse>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CheckoutCommandHandler> _logger;

    public CheckoutCommandHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        ILogger<CheckoutCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<MessageResponse>> Handle(CheckoutCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing checkout for customer {CustomerId}", request.CustomerId);

            // Get pending orders for this customer
            var orders = await _orderRepository.GetByCustomerIdAsync(request.CustomerId, cancellationToken);
            var pendingOrders = orders.Where(o =>
                o.Status == OrderStatus.Pending ||
                o.Status == OrderStatus.PaymentPending ||
                o.Status == OrderStatus.Confirmed).ToList();

            if (!pendingOrders.Any())
            {
                return Result<MessageResponse>.Failure(
                    "You don't have any pending orders to checkout. 🛒\n\n" +
                    "Would you like to place a new order?",
                    "ValidationError"
                );
            }

            var totalAmount = pendingOrders.Sum(o => o.TotalAmount);
            var orderNumbers = string.Join(", ", pendingOrders.Select(o => o.OrderNumber));
            var orderCount = pendingOrders.Count;

            // Update orders to payment pending
            foreach (var order in pendingOrders)
            {
                order.RequestPayment();
                await _orderRepository.UpdateAsync(order, cancellationToken);
            }
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = $"💳 **Checkout Summary**\n\n" +
                          $"You have {orderCount} order(s) ready for payment:\n" +
                          $"📦 **Orders:** {orderNumbers}\n" +
                          $"💰 **Total Amount:** {totalAmount::N2}\n\n" +
                          $"🔐 **Payment Options:**\n" +
                          $"1. Pay with Card (Stripe)\n" +
                          $"2. Pay with Mobile Money (Flutterwave)\n" +
                          $"3. Cash on Delivery\n\n" +
                          $"Reply with your preferred payment method:\n" +
                          $"• `PAY CARD` - Pay with Credit/Debit Card\n" +
                          $"• `PAY MOBILE` - Pay with Mobile Money\n" +
                          $"• `PAY COD` - Cash on Delivery\n\n" +
                          $"Or reply `CANCEL` to cancel the checkout.";

          
            return Result<MessageResponse>.Success(
    MessageResponse.Success(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing checkout for customer {CustomerId}", request.CustomerId);
            return Result<MessageResponse>.Failure($"Failed to process checkout: {ex.Message}");
        }
    }
}