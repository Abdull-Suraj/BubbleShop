// Application/Features/Orders/Commands/CancelOrder/CancelOrderCommandHandler.cs
using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Application.Common.Models;
using BubbleShop.Domain.Enums;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BubbleShop.Application.Features.Orders.Commands.CancelOrder;

public sealed class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, Result>
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

    public async Task<Result> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Cancelling order: {OrderId}", request.OrderId);

            var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order is null)
                return Result.Failure($"Order {request.OrderId} not found", "NotFound");

            // Check if order can be cancelled
            if (order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Completed)
                return Result.Failure("Cannot cancel a delivered or completed order", "ValidationError");

            if (order.Status == OrderStatus.Shipped)
                return Result.Failure("Cannot cancel a shipped order. Please contact support.", "ValidationError");

            // Cancel the order with reason
            order.Cancel(request.Reason ?? "Cancelled by customer");
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Order cancelled successfully: {OrderId}", request.OrderId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling order: {OrderId}", request.OrderId);
            return Result.Failure($"Failed to cancel order: {ex.Message}");
        }
    }
}