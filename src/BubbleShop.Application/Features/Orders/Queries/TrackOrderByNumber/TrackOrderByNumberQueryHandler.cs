// Application/Features/Orders/Queries/TrackOrderByNumber/TrackOrderByNumberQueryHandler.cs
using BubbleShop.Application.Common.Models;
using BubbleShop.Application.DTOs;
using BubbleShop.Domain.Entities;
using BubbleShop.Domain.Enums;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BubbleShop.Application.Features.Orders.Queries.TrackOrderByNumber;

public sealed class TrackOrderByNumberQueryHandler : IRequestHandler<TrackOrderByNumberQuery, Result<TrackingInfoDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<TrackOrderByNumberQueryHandler> _logger;

    public TrackOrderByNumberQueryHandler(
        IOrderRepository orderRepository,
        ILogger<TrackOrderByNumberQueryHandler> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task<Result<TrackingInfoDto>> Handle(TrackOrderByNumberQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Tracking order: {OrderNumber}", request.OrderNumber);

            var order = await _orderRepository.GetByOrderNumberAsync(request.OrderNumber, cancellationToken);
            if (order is null)
                return Result<TrackingInfoDto>.Failure($"Order {request.OrderNumber} not found", "NotFound");

            // Optional email verification
            if (!string.IsNullOrEmpty(request.Email) && order.CustomerEmail != request.Email)
                return Result<TrackingInfoDto>.Failure("Order not found for this email", "NotFound");

            var trackingInfo = new TrackingInfoDto
            {
                OrderNumber = order.OrderNumber,
                OrderId = order.Id,
                Status = order.Status.ToString(),
                StatusDisplay = GetStatusDisplay(order.Status),
                ProgressPercentage = GetProgressPercentage(order.Status),
                TotalAmount = order.TotalAmount,
                CreatedAt = order.CreatedAt,
                EstimatedDelivery = order.ShippedAt?.AddDays(3) ?? order.CreatedAt.AddDays(5),
                CurrentLocation = GetCurrentLocation(order),
                TrackingHistory = GetTrackingHistory(order),
                Items = order.OrderItems.Select(i => new OrderItemTrackingDto
                {
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };

            return Result<TrackingInfoDto>.Success(trackingInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking order: {OrderNumber}", request.OrderNumber);
            return Result<TrackingInfoDto>.Failure($"Failed to track order: {ex.Message}");
        }
    }

    private string GetStatusDisplay(OrderStatus status) => status switch
    {
        OrderStatus.Pending => "Order Placed",
        OrderStatus.PaymentPending => "Awaiting Payment",
        OrderStatus.PaymentReceived => "Payment Confirmed",
        OrderStatus.Confirmed => "Order Confirmed",
        OrderStatus.Processing => "Being Processed",
        OrderStatus.Shipped => "On The Way",
        OrderStatus.Delivered => "Delivered",
        OrderStatus.Completed => "Completed",
        OrderStatus.Cancelled => "Cancelled",
        _ => status.ToString()
    };

    private int GetProgressPercentage(OrderStatus status) => status switch
    {
        OrderStatus.Pending => 10,
        OrderStatus.PaymentPending => 20,
        OrderStatus.PaymentReceived => 30,
        OrderStatus.Confirmed => 40,
        OrderStatus.Processing => 60,
        OrderStatus.Shipped => 80,
        OrderStatus.Delivered => 95,
        OrderStatus.Completed => 100,
        OrderStatus.Cancelled => 0,
        _ => 0
    };

    private List<TrackingHistoryDto> GetTrackingHistory(Order order)
    {
        var history = new List<TrackingHistoryDto>();

        if (order.CreatedAt != default)
            history.Add(new TrackingHistoryDto { Status = "Order Placed", Description = "Your order has been received", Timestamp = order.CreatedAt });

        if (order.PaidAt.HasValue)
            history.Add(new TrackingHistoryDto { Status = "Payment Confirmed", Description = "Payment has been confirmed", Timestamp = order.PaidAt.Value });

        if (order.ConfirmedAt.HasValue)
            history.Add(new TrackingHistoryDto { Status = "Order Confirmed", Description = "Your order has been confirmed", Timestamp = order.ConfirmedAt.Value.ToLocalTime() });

        if (order.ShippedAt.HasValue)
            history.Add(new TrackingHistoryDto { Status = "Order Shipped", Description = "Your order has been shipped", Timestamp = order.ShippedAt.Value.ToLocalTime() });

        if (order.DeliveredAt.HasValue)
            history.Add(new TrackingHistoryDto { Status = "Order Delivered", Description = "Your order has been delivered", Timestamp = order.DeliveredAt.Value.ToLocalTime() });

        return history.OrderBy(h => h.Timestamp).ToList();
    }

    private string GetCurrentLocation(Order order)
    {
        if (order.DeliveredAt.HasValue)
            return "Delivered to customer";
        if (order.ShippedAt.HasValue)
            return "In transit to destination";
        if (order.ConfirmedAt.HasValue)
            return "Being prepared for shipping";
        return "Processing at warehouse";
    }
}