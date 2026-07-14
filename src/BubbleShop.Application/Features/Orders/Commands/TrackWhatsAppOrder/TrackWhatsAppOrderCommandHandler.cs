using BubbleShop.Application.Common.Models;
using BubbleShop.Domain.Entities;
using BubbleShop.Domain.Enums;
using BubbleShop.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BubbleShop.Application.Features.Orders.Commands.TrackWhatsAppOrder;

public sealed class TrackWhatsAppOrderCommandHandler : IRequestHandler<TrackWhatsAppOrderCommand, Result<WhatsAppOrderTrackingResponse>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<TrackWhatsAppOrderCommandHandler> _logger;

    public TrackWhatsAppOrderCommandHandler(
        IOrderRepository orderRepository,
        ILogger<TrackWhatsAppOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task<Result<WhatsAppOrderTrackingResponse>> Handle(TrackWhatsAppOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Tracking order {OrderNumber} for {CustomerWhatsApp}",
                request.OrderNumber, request.CustomerWhatsApp);

            var order = await _orderRepository.GetByOrderNumberAsync(request.OrderNumber, cancellationToken);
            if (order is null)
                return Result<WhatsAppOrderTrackingResponse>.Failure($"Order {request.OrderNumber} not found", "NotFound");

            // Verify customer ownership (optional security)
            if (order.CustomerWhatsApp != request.CustomerWhatsApp)
                return Result<WhatsAppOrderTrackingResponse>.Failure("Order not found for this customer", "NotFound");

            var response = new WhatsAppOrderTrackingResponse(
                OrderNumber: order.OrderNumber,
                Status: order.Status.ToString(),
                StatusDisplay: GetStatusDisplay(order.Status),
                ProgressPercentage: GetProgressPercentage(order.Status),
                CreatedAt: order.CreatedAt,
                EstimatedDelivery: order.ShippedAt?.AddDays(2) ?? order.CreatedAt.AddDays(3),
                Timeline: GetTimeline(order)
            );

            return Result<WhatsAppOrderTrackingResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking order {OrderNumber}", request.OrderNumber);
            return Result<WhatsAppOrderTrackingResponse>.Failure($"Failed to track order: {ex.Message}");
        }
    }

    private string GetStatusDisplay(OrderStatus status) => status switch
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

    private List<string> GetTimeline(Order order)
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

        return timeline;
    }
}