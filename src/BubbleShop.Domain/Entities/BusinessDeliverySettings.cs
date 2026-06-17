namespace BubbleShop.Domain.Entities;

public class BusinessDeliverySettings
{
    public bool IsDeliveryEnabled { get; set; } = true;
    public decimal DeliveryFee { get; set; } = 5.00m;
    public decimal FreeDeliveryThreshold { get; set; } = 50.00m;
    public int DefaultDeliveryTimeInHours { get; set; } = 48;
    public bool IsPickupAvailable { get; set; } = true;
    public List<string> DeliveryAreas { get; set; } = new();
    public Dictionary<string, decimal> ZoneDeliveryFees { get; set; } = new();

    public decimal CalculateDeliveryFee(decimal subtotal, string? deliveryZone = null)
    {
        // Free delivery for orders above threshold
        if (FreeDeliveryThreshold > 0 && subtotal >= FreeDeliveryThreshold)
            return 0;

        // Check if zone-based pricing applies
        if (!string.IsNullOrEmpty(deliveryZone) && ZoneDeliveryFees.ContainsKey(deliveryZone))
            return ZoneDeliveryFees[deliveryZone];

        return DeliveryFee;
    }

    public bool IsDeliveryAvailable(string location)
    {
        if (!IsDeliveryEnabled) return false;
        if (!DeliveryAreas.Any()) return true;

        return DeliveryAreas.Any(area =>
            location.Contains(area, StringComparison.OrdinalIgnoreCase));
    }
}
