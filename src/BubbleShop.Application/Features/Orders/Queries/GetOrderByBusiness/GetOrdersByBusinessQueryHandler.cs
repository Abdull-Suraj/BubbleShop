using BubbleShop.Application.Common.Models;
using BubbleShop.Application.DTOs;
using BubbleShop.Domain.Enums;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BubbleShop.Application.Features.Orders.Queries.GetOrdersByBusiness;

public sealed class GetOrdersByBusinessQueryHandler : IRequestHandler<GetOrdersByBusinessQuery, Result<PagedResult<OrderSummaryDto>>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<GetOrdersByBusinessQueryHandler> _logger;

    public GetOrdersByBusinessQueryHandler(
        IOrderRepository orderRepository,
        ILogger<GetOrdersByBusinessQueryHandler> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task<Result<PagedResult<OrderSummaryDto>>> Handle(GetOrdersByBusinessQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting orders for business: {BusinessId}", request.BusinessId);

            var allOrders = await _orderRepository.GetByBusinessIdAsync(request.BusinessId, cancellationToken);
            var ordersList = allOrders.ToList();

            // Apply status filter
            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                ordersList = ordersList.Where(o => o.Status.ToString().Equals(request.Status, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            var totalCount = ordersList.Count;
            var pagedOrders = ordersList
                .OrderByDescending(o => o.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var orderSummaries = pagedOrders.Select(o => new OrderSummaryDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                CustomerName = o.CustomerName,          // Now exists
                CustomerWhatsApp = o.CustomerWhatsApp,  // Now exists
                TotalAmount = o.TotalAmount,
                Status = o.Status.ToString(),
                StatusDisplay = GetStatusDisplay(o.Status),
                StatusColor = GetStatusColor(o.Status),
                CreatedAt = o.CreatedAt.Date,       // Convert to DateTime
                ItemCount = o.OrderItems.Count
            }).ToList();

            var result = new PagedResult<OrderSummaryDto>
            {
                Items = orderSummaries,
                TotalCount = totalCount,
                Page = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
            };

            return Result<PagedResult<OrderSummaryDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orders for business: {BusinessId}", request.BusinessId);
            return Result<PagedResult<OrderSummaryDto>>.Failure($"Failed to retrieve orders: {ex.Message}");
        }
    }

    private string GetStatusDisplay(OrderStatus status) => status switch
    {
        OrderStatus.Pending => "Order Received",
        OrderStatus.PaymentPending => "Awaiting Payment",
        OrderStatus.PaymentReceived => "Payment Confirmed",
        OrderStatus.Confirmed => "Order Confirmed",
        OrderStatus.Processing => "Preparing Order",
        OrderStatus.Shipped => "Order Shipped",
        OrderStatus.Delivered => "Order Delivered",
        OrderStatus.Completed => "Order Completed",
        OrderStatus.Cancelled => "Order Cancelled",
        _ => status.ToString()
    };

    private string GetStatusColor(OrderStatus status) => status switch
    {
        OrderStatus.Pending => "#FFA500",
        OrderStatus.PaymentPending => "#FFD700",
        OrderStatus.PaymentReceived => "#4CAF50",
        OrderStatus.Confirmed => "#2196F3",
        OrderStatus.Processing => "#9C27B0",
        OrderStatus.Shipped => "#00BCD4",
        OrderStatus.Delivered => "#4CAF50",
        OrderStatus.Completed => "#8BC34A",
        OrderStatus.Cancelled => "#F44336",
        _ => "#9E9E9E"
    };
}