
using BubbleShop.Application.Common.Models;
using MediatR;

namespace BubbleShop.Application.Features.Orders.Commands.TrackWhatsAppOrder;

public sealed record TrackWhatsAppOrderCommand(
    string OrderNumber,
    string CustomerWhatsApp
) : IRequest<Result<WhatsAppOrderTrackingResponse>>;

public record WhatsAppOrderTrackingResponse(
    string OrderNumber,
    string Status,
    string StatusDisplay,
    int ProgressPercentage,
    DateTime CreatedAt,
    DateTime? EstimatedDelivery,
    List<string> Timeline
);