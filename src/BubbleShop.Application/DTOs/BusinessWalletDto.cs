namespace BubbleShop.Application.DTOs;

public class BusinessWalletDto
{
    public Guid BusinessId { get; set; }
    public string BusinessName { get; set; } = string.Empty;
    public string Currency { get; set; } = "USD";
    public decimal AvailableBalance { get; set; }
    public decimal PendingSettlement { get; set; }
    public decimal TotalEarned { get; set; }
    public DateTime LastUpdated { get; set; }
}