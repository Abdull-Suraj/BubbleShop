// Domain/Entities/ProductOption.cs
namespace BubbleShop.Domain.Entities;

/// <summary>
/// Product Option for customization
/// </summary>
public class ProductOption
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public decimal PriceAdjustment { get; set; }

    public string PriceAdjustmentFormatted => PriceAdjustment != 0
        ? $"{(PriceAdjustment > 0 ? "+" : "")}{PriceAdjustment:C}"
        : null;

    public ProductOption() { }

    public ProductOption(string name, string value, decimal priceAdjustment = 0)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Value = value ?? throw new ArgumentNullException(nameof(value));
        PriceAdjustment = priceAdjustment;
    }
}
