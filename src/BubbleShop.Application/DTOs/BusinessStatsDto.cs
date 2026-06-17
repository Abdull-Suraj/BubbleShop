namespace BubbleShop.Application.DTOs;

public class BusinessStatsDto
{
    // Order Statistics
    public int TotalOrders { get; set; }
    public int CompletedOrders { get; set; }
    public int PendingOrders { get; set; }
    public int CancelledOrders { get; set; }

    // Revenue Statistics
    public decimal TotalRevenue { get; set; }
    public decimal PlatformFees { get; set; }
    public decimal NetRevenue { get; set; }
    public decimal AverageOrderValue { get; set; }

    // Product Statistics
    public int TotalProducts { get; set; }
    public int ActiveProducts { get; set; }
    public int OutOfStockProducts { get; set; }
    public int LowStockProducts { get; set; }

    // Customer Statistics
    public int TotalCustomers { get; set; }
    public int NewCustomersThisMonth { get; set; }

    // Recent Orders
    public List<RecentOrderDto> RecentOrders { get; set; } = new();

    // Period
    public DateTime? PeriodStart { get; set; }
    public DateTime? PeriodEnd { get; set; }
}

public class RecentOrderDto
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}