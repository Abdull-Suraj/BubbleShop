using BubbleShop.Application.DTOs;

namespace BubbleShop.Application.DTOs;

public class TrackingInfoDto
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusDisplay { get; set; } = string.Empty;
    public int ProgressPercentage { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? EstimatedDelivery { get; set; }
    public string CurrentLocation { get; set; } = string.Empty;
    public List<TrackingHistoryDto> TrackingHistory { get; set; } = new();
    public List<OrderItemTrackingDto> Items { get; set; } = new();
}

public class TrackingHistoryDto
{
    public string Status { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class OrderItemTrackingDto
{
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}