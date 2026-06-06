using BubbleShop.Domain.Exceptions;

namespace BubbleShop.Domain.Entities;

public sealed class Product : BaseEntity
{
    private Product()
    {
    }

    private Product(Guid id, string name, string description, decimal price, int stockQuantity, string? imageUrl)
    {
        if (price < 0)
        {
            throw new DomainException("Price cannot be negative.");
        }

        if (stockQuantity < 0)
        {
            throw new DomainException("Stock cannot be negative.");
        }

        Id = id;
        Name = name;
        Description = description;
        Price = price;
        StockQuantity = stockQuantity;
        ImageUrl = imageUrl;
        IsActive = true;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int StockQuantity { get; private set; }
    public string? ImageUrl { get; private set; }
    public bool IsActive { get; private set; }

    public static Product Create(string name, string description, decimal price, int stockQuantity, string? imageUrl)
        => new(Guid.NewGuid(), name, description, price, stockQuantity, imageUrl);

    public void Update(string name, string description, decimal price, string? imageUrl, bool isActive)
    {
        if (price < 0)
        {
            throw new DomainException("Price cannot be negative.");
        }

        Name = name;
        Description = description;
        Price = price;
        ImageUrl = imageUrl;
        IsActive = isActive;
    }

    public void UpdateStock(int quantity)
    {
        if (quantity < 0)
        {
            throw new DomainException("Stock cannot be negative.");
        }

        StockQuantity = quantity;
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
    }
}
