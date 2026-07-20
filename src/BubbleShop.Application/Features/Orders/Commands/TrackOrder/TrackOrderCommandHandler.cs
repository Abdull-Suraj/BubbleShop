// Application/Features/Orders/Commands/TrackOrder/TrackOrderCommandHandler.cs
using BubbleShop.Application.AppServices;
using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Application.Common.Models;
using BubbleShop.Domain.Entities;
using BubbleShop.Domain.Enums;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BubbleShop.Application.Features.Orders.Commands.TrackOrder;

public sealed class TrackOrderCommandHandler : IRequestHandler<TrackOrderCommand, Result<MessageResponse>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<TrackOrderCommandHandler> _logger;

    public TrackOrderCommandHandler(
        IOrderRepository orderRepository,
        ILogger<TrackOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task<Result<MessageResponse>> Handle(TrackOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Tracking order {OrderNumber} for customer {CustomerId}",
                request.OrderNumber, request.CustomerId);

            if (string.IsNullOrWhiteSpace(request.OrderNumber))
            {
                return Result<MessageResponse>.Failure(
                    "Please provide your order number. You can find it in your order confirmation message.",
                    "ValidationError"
                );
            }

            var order = await _orderRepository.GetByOrderNumberAsync(request.OrderNumber, cancellationToken);

            if (order is null)
            {
                return Result<MessageResponse>.Failure(
                    $"Order '{request.OrderNumber}' not found. Please check the number and try again.",
                    "NotFound"
                );
            }

            // Verify customer owns this order
            if (order.CustomerWhatsApp != request.CustomerId && order.CustomerId.ToString() != request.CustomerId)
            {
                return Result<MessageResponse>.Failure(
                    "Order not found for this customer. Please check your order number.",
                    "Unauthorized"
                );
            }

            var statusEmoji = GetStatusEmoji(order.Status);
            var statusDisplay = GetStatusDisplay(order.Status);
            var progress = GetProgressPercentage(order.Status);

            var response = $"{statusEmoji} **Order Status**\n\n" +
                          $"📦 **Order #{order.OrderNumber}**\n" +
                          $"📊 **Status:** {statusDisplay}\n" +
                          $"💰 **Total:** {order.TotalAmount:C}\n" +
                          $"📅 **Date:** {order.CreatedAt:dddd, MMMM d, yyyy}\n" +
                          $"🔄 **Progress:** {progress}%\n\n";

            // Add timeline
            response += GetTimeline(order);

            // Add delivery estimate
            if (order.Status != OrderStatus.Delivered && order.Status != OrderStatus.Completed)
            {
                var estimatedDelivery = order.CreatedAt.AddDays(3);
                response += $"\n📅 **Estimated Delivery:** {estimatedDelivery:dddd, MMMM d, yyyy}\n";
            }

            response += $"\n💬 Reply `MENU` to see other options.";

            return Result<MessageResponse>.Success( MessageResponse.Success(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking order {OrderNumber}", request.OrderNumber);
            return Result<MessageResponse>.Failure($"Failed to track order: {ex.Message}");
        }
    }

    private string GetStatusEmoji(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Pending => "⏳",
            OrderStatus.PaymentPending => "💰",
            OrderStatus.PaymentReceived => "✅",
            OrderStatus.Confirmed => "👍",
            OrderStatus.Processing => "🔧",
            OrderStatus.Shipped => "🚚",
            OrderStatus.Delivered => "📦",
            OrderStatus.Completed => "✨",
            OrderStatus.Cancelled => "❌",
            _ => "📋"
        };
    }

    private string GetStatusDisplay(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Pending => "Order Received",
            OrderStatus.PaymentPending => "Awaiting Payment",
            OrderStatus.PaymentReceived => "Payment Confirmed",
            OrderStatus.Confirmed => "Order Confirmed",
            OrderStatus.Processing => "Being Prepared",
            OrderStatus.Shipped => "On The Way",
            OrderStatus.Delivered => "Delivered",
            OrderStatus.Completed => "Completed",
            OrderStatus.Cancelled => "Cancelled",
            _ => status.ToString()
        };
    }

    private int GetProgressPercentage(OrderStatus status)
    {
        return status switch
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
    }

    private string GetTimeline(Order order)
    {
        var timeline = new List<string>();

        if (order.CreatedAt != default)
            timeline.Add($"📋 Order placed: {order.CreatedAt:MMM dd, yyyy HH:mm}");

        if (order.PaidAt.HasValue)
            timeline.Add($"✅ Payment confirmed: {order.PaidAt.Value:MMM dd, yyyy HH:mm}");

        if (order.ConfirmedAt.HasValue)
            timeline.Add($"👍 Order confirmed: {order.ConfirmedAt.Value:MMM dd, yyyy HH:mm}");

        if (order.ShippedAt.HasValue)
            timeline.Add($"🚚 Order shipped: {order.ShippedAt.Value:MMM dd, yyyy HH:mm}");

        if (order.DeliveredAt.HasValue)
            timeline.Add($"📦 Order delivered: {order.DeliveredAt.Value:MMM dd, yyyy HH:mm}");

        if (order.CancelledAt.HasValue)
            timeline.Add($"❌ Order cancelled: {order.CancelledAt.Value:MMM dd, yyyy HH:mm}");

        return string.Join("\n", timeline);
    }
}