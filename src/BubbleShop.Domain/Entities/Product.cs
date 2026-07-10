using BubbleShop.Domain.Common;
using BubbleShop.Domain.Exceptions;

namespace BubbleShop.Domain.Entities;

public sealed class Product : BaseEntity
{
    private Product()
    {

    }

    private Product(Guid id, string name, string description, decimal price, int stockQuantity, string? imageUrl, string? category = null, string? sku = null, Guid? businessId = null)
    {
        if (price < 0)
        {
            throw new DomainException("Price cannot be negative.");
        }

        if (stockQuantity < 0)
        {
            throw new DomainException("Stock cannot be negative.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Product name is required.");
        }

        Id = id;
        BusinessId = businessId ?? Guid.Empty;
        Name = name;
        Description = description ?? string.Empty;
        Price = price;
        StockQuantity = stockQuantity;
        ImageUrl = imageUrl;
        Category = category ?? "Uncategorized";
        SKU = sku ?? GenerateSKU(name);
        IsActive = true;
        IsDeleted = false;
        Images = new List<string>();
        Tags = new List<string>();
        CreatedAt = DateTime.UtcNow;
    }


    public Guid BusinessId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string SKU { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public decimal CompareAtPrice { get; private set; }
    public decimal Cost { get; private set; }
    public int StockQuantity { get; private set; }
    public int ReservedQuantity { get; private set; }
    public string Category { get; private set; } = string.Empty;
    public string? ImageUrl { get; private set; }
    public List<string> Images { get; private set; } = [];
    public List<string> Tags { get; private set; } = [];
    public string ThumbnailUrl { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public bool IsDigital { get; private set; }
    public string DigitalFileUrl { get; private set; } = string.Empty;
    public int LowStockThreshold { get; private set; } = 10;

    // Navigation Properties
    public Business Business { get; private set; } = null!;
    private readonly List<OrderItem> _orderItems = [];
    public IReadOnlyCollection<OrderItem> OrderItems => _orderItems;

    // Factory Methods
    public static Product Create(string name, string description, decimal price, int stockQuantity, string? imageUrl, string? category = null, Guid? businessId = null)
        => new(Guid.NewGuid(), name, description, price, stockQuantity, imageUrl, category, null, businessId);

    public static Product Create(Guid businessId, string name, string description, decimal price, int stockQuantity, string? imageUrl, string? category = null)
        => new(Guid.NewGuid(), name, description, price, stockQuantity, imageUrl, category, null, businessId);

    // Update Methods
    public void Update(string name, string description, decimal price, string? imageUrl, bool isActive)
    {
        if (price < 0)
        {
            throw new DomainException("Price cannot be negative.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Product name is required.");
        }

        Name = name;
        Description = description;
        Price = price;
        ImageUrl = imageUrl;
        IsActive = isActive;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string name, string description, string? category, string? imageUrl, List<string>? tags = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Product name is required.");
        }

        Name = name;
        Description = description;
        Category = category ?? Category;
        ImageUrl = imageUrl;
        if (tags != null)
        {
            Tags = tags;
        }
        LastModifiedAt = DateTime.UtcNow;
    }

    public void UpdatePrice(decimal price)
    {
        if (price < 0)
        {
            throw new DomainException("Price cannot be negative.");
        }

        Price = price;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void UpdateStock(int quantity)
    {
        if (quantity < 0)
        {
            throw new DomainException("Stock cannot be negative.");
        }

        StockQuantity = quantity;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void ReduceStock(int quantity)
    {
        if (quantity <= 0)
        {
            throw new DomainException("Quantity must be greater than zero.");
        }

        if (StockQuantity < quantity)
        {
            throw new ProductOutOfStockException(Id);
        }

        StockQuantity -= quantity;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void ReserveStock(int quantity)
    {
        if (quantity <= 0)
        {
            throw new DomainException("Quantity must be greater than zero.");
        }

        if (AvailableStock < quantity)
        {
            throw new DomainException($"Insufficient available stock. Available: {AvailableStock}");
        }

        ReservedQuantity += quantity;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void ReleaseReservedStock(int quantity)
    {
        if (quantity <= 0)
        {
            throw new DomainException("Quantity must be greater than zero.");
        }

        if (ReservedQuantity < quantity)
        {
            throw new DomainException($"Cannot release more than reserved. Reserved: {ReservedQuantity}");
        }

        ReservedQuantity -= quantity;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void UpdateCategory(string category)
    {
        Category = category;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void UpdateImages(List<string> images, string? thumbnailUrl = null)
    {
        Images = images ?? new List<string>();
        ThumbnailUrl = thumbnailUrl ?? images?.FirstOrDefault() ?? string.Empty;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void UpdateSKU(string sku)
    {
        if (string.IsNullOrWhiteSpace(sku))
        {
            throw new DomainException("SKU cannot be empty.");
        }

        SKU = sku;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void MarkAsDigital(string fileUrl)
    {
        IsDigital = true;
        DigitalFileUrl = fileUrl;
        LastModifiedAt = DateTime.UtcNow;
    }

    // Properties
    public int AvailableStock => StockQuantity - ReservedQuantity;
    public bool HasDiscount => CompareAtPrice > 0;
    public decimal DiscountPercentage => HasDiscount ? ((CompareAtPrice - Price) / CompareAtPrice) * 100 : 0;
    public bool IsInStock => StockQuantity > 0;
    public bool IsLowStock => StockQuantity > 0 && StockQuantity <= LowStockThreshold;

    // Helper Methods
    private static string GenerateSKU(string name)
    {
        var prefix = name.Length >= 3 ? name[..3].ToUpper() : name.ToUpper();
        return $"{prefix}-{Guid.NewGuid():N}"[..8].ToUpper();
    }

    public override string ToString()
    {
        return $"{Name} ({SKU}) - ${Price}";
    }
}