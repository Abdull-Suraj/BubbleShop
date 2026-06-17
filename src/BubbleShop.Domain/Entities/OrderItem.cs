// Domain/Entities/OrderItem.cs
using BubbleShop.Domain.Common;
using BubbleShop.Domain.Exceptions;

namespace BubbleShop.Domain.Entities;

public class OrderItem : BaseEntity
{
    // Core Identifiers
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }

    // Product Snapshot - ADD THIS
    public string ProductName { get; private set; } = string.Empty;  // ← ADD THIS

    public string ProductSKU { get; private set; } = string.Empty;
    public string ProductImage { get; private set; } = string.Empty;
    public string ProductImageThumbnail { get; private set; } = string.Empty;

    // Quantity and Pricing
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal OriginalUnitPrice { get; private set; }
    public decimal TotalPrice { get; private set; }


    // Options
    public List<ProductOption> SelectedOptions { get; private set; } = new();

    // Navigation Properties
    public Order Order { get; private set; } = null!;
    public Product Product { get; private set; } = null!;

    private OrderItem() { }

    // Constructor with ProductName
    public OrderItem(
        Guid productId,
        string productName,  // ← Accept product name
        int quantity,
        decimal unitPrice,
        string? productSKU = null,
        string? productImage = null,
        List<ProductOption>? options = null,
        decimal discountPercentage = 0,
        decimal taxRate = 0.10m)
    {
        Id = Guid.NewGuid();
        ProductId = productId;
        ProductName = productName ?? throw new ArgumentNullException(nameof(productName));  // ← Set it
        ProductSKU = productSKU ?? string.Empty;
        ProductImage = productImage ?? string.Empty;
        ProductImageThumbnail = productImage ?? string.Empty;
        Quantity = quantity;
        UnitPrice = unitPrice;
        OriginalUnitPrice = unitPrice;

        SelectedOptions = options ?? new List<ProductOption>();
        CreatedAt = DateTime.UtcNow;
        CalculateTotals();
    }

    // Constructor with OrderId
    public OrderItem(
        Guid orderId,
        Guid productId,
        string productName,  // ← Accept product name
        int quantity,
        decimal unitPrice,
        string? productSKU = null,
        string? productImage = null,
        List<ProductOption>? options = null)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        ProductId = productId;
        ProductName = productName ?? throw new ArgumentNullException(nameof(productName));  // ← Set it
        ProductSKU = productSKU ?? string.Empty;
        ProductImage = productImage ?? string.Empty;
        ProductImageThumbnail = productImage ?? string.Empty;
        Quantity = quantity;
        UnitPrice = unitPrice;
        OriginalUnitPrice = unitPrice;
        SelectedOptions = options ?? new List<ProductOption>();
        CreatedAt = DateTime.UtcNow;
        CalculateTotals();
    }
    public static OrderItem Create(
        Guid productId,
        string productName,
        int quantity,
        decimal unitPrice,
        string? productSKU = null,
        string? productImage = null,
        List<ProductOption>? options = null)
    {
        return new OrderItem(
            productId: productId,
            productName: productName,
            quantity: quantity,
            unitPrice: unitPrice,
            productSKU: productSKU,
            productImage: productImage,
            options: options
        );
    }

    public static OrderItem Create(
        Guid orderId,
        Guid productId,
        int quantity,
        decimal unitPrice,
        string? productName = null,
        string? productSKU = null,
        string? productImage = null)
    {
        return new OrderItem(
            orderId: orderId,
            productId: productId,
            productName: productName ?? "Unknown Product",
            quantity: quantity,
            unitPrice: unitPrice,
            productSKU: productSKU,
            productImage: productImage
        );
    }

    private void CalculateTotals()
    {
        var subtotal = UnitPrice * Quantity;


        TotalPrice = subtotal;
    }

    private decimal CalculateItemDiscount()
    {
        if (Quantity >= 10)
            return (UnitPrice * Quantity) * 0.10m;
        if (Quantity >= 5)
            return (UnitPrice * Quantity) * 0.05m;
        return 0;
    }

    public void UpdateQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be positive");
        if (quantity > 100)
            throw new DomainException("Maximum quantity per item is 100");
        Quantity = quantity;
        CalculateTotals();
        LastModifiedAt = DateTime.UtcNow;
    }

    public void UpdateUnitPrice(decimal newUnitPrice)
    {
        if (newUnitPrice < 0)
            throw new DomainException("Unit price cannot be negative");
        UnitPrice = newUnitPrice;
        CalculateTotals();
        LastModifiedAt = DateTime.UtcNow;
    }


    public void UpdateProductImage(string imageUrl, string? thumbnailUrl = null)
    {
        ProductImage = imageUrl;
        ProductImageThumbnail = thumbnailUrl ?? imageUrl;
        LastModifiedAt = DateTime.UtcNow;
    }

    public decimal Subtotal => UnitPrice * Quantity;
    public decimal LineTotal => TotalPrice;
    
    public decimal Savings => (OriginalUnitPrice - UnitPrice) * Quantity;
   
    public bool HasOptions => SelectedOptions != null && SelectedOptions.Any();
    public string OptionsSummary => HasOptions ? string.Join(", ", SelectedOptions.Select(o => $"{o.Name}: {o.Value}")) : "None";

    public override string ToString()
    {
        return $"{Quantity}x {ProductName} @ {UnitPrice:C} = {TotalPrice:C}";
    }
}

public class ProductOption
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public decimal PriceAdjustment { get; set; }
}