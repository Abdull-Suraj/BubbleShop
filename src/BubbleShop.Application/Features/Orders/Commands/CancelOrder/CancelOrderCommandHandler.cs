// Application/Features/Orders/Commands/CancelOrder/CancelOrderCommandHandler.cs
using BubbleShop.Application.AppServices;
using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Application.Common.Models;
using BubbleShop.Domain.Enums;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BubbleShop.Application.Features.Orders.Commands.CancelOrder;

public sealed class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, Result<MessageResponse>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CancelOrderCommandHandler> _logger;

    public CancelOrderCommandHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        ILogger<CancelOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<MessageResponse>> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Cancelling order {OrderNumber} for customer {CustomerId}",
                request.OrderId, request.CustomerId);

            if (string.IsNullOrWhiteSpace(request.OrderId))
            {
                return Result<MessageResponse>.Failure(
                    "Please provide the order number you want to cancel.",
                    "ValidationError"
                );
            }

            var order = await _orderRepository.GetByOrderNumberAsync(request.OrderId, cancellationToken);

            if (order is null)
            {
                return Result<MessageResponse>.Failure(
                    $"Order '{request.OrderId}' not found.",
                    "NotFound"
                );
            }

            // Verify customer owns this order
            if (order.CustomerWhatsApp != request.CustomerId && order.CustomerId.ToString() != request.CustomerId)
            {
                return Result<MessageResponse>.Failure(
                    "Order not found for this customer.",
                    "Unauthorized"
                );
            }

            // Check if order can be cancelled
            if (order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Completed)
            {
                return Result<MessageResponse>.Failure(
                    "This order has already been delivered and cannot be cancelled.",
                    "ValidationError"
                );
            }

            if (order.Status == OrderStatus.Shipped)
            {
                return Result<MessageResponse>.Failure(
                    "This order has already been shipped. Please contact support for assistance.",
                    "ValidationError"
                );
            }

            if (order.Status == OrderStatus.Cancelled)
            {
                return Result<MessageResponse>.Failure(
                    "This order has already been cancelled.",
                    "ValidationError"
                );
            }

            // Cancel the order
            order.Cancel(request.Reason);
            await _orderRepository.UpdateAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = $"✅ **Order Cancelled**\n\n" +
                          $"Your order #{order.OrderNumber} has been cancelled.\n" +
                          $"Reason: {request.Reason}\n\n" +
                          $"If you need to place a new order, just say 'I want to buy [product]'.\n\n" +
                          $"Thank you for understanding! 🙏";

            return Result<MessageResponse>.Success( MessageResponse.Success(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling order {OrderNumber}", request.OrderId);
            return Result<MessageResponse>.Failure($"Failed to cancel order: {ex.Message}");
        }
    }
}