// Domain/Entities/CartItem.cs
using BubbleShop.Domain.Common;
using BubbleShop.Domain.Exceptions;

namespace BubbleShop.Domain.Entities;


public class CartItem : BaseEntity
{
    // Core Identifiers
    public Guid CartId { get; private set; }
    public Guid ProductId { get; private set; }

    // Product Snapshot (preserved even if product changes later)
    public string ProductName { get; private set; } = string.Empty;
    public string ProductSKU { get; private set; } = string.Empty;
    public string? ProductImage { get; private set; }
    public string? ProductImageThumbnail { get; private set; }

    // Quantity and Pricing
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TotalPrice => UnitPrice * Quantity;



    // Options (for customizable products)
    public List<ProductOption> SelectedOptions { get; private set; } = new();

    // Product Details (additional snapshot data)
    public string? ProductDescription { get; private set; }
    public string? ProductCategory { get; private set; }




    // Navigation Properties
    public Cart Cart { get; private set; } = null!;
    public Product Product { get; private set; } = null!;

    private CartItem() { }

    // Main constructor for creating a cart item
    public CartItem(
        Guid cartId,
        Guid productId,
        string productName,
        int quantity,
        decimal unitPrice,
        string? productSKU = null,
        string? productImage = null,
        List<ProductOption>? options = null,
        decimal discountPercentage = 0,
        decimal taxRate = 0.10m)
    {
        Id = Guid.NewGuid();
        CartId = cartId;
        ProductId = productId;
        ProductName = productName ?? throw new ArgumentNullException(nameof(productName));
        ProductSKU = productSKU ?? string.Empty;
        ProductImage = productImage;
        ProductImageThumbnail = productImage;
        Quantity = quantity;
        UnitPrice = unitPrice;
        SelectedOptions = options ?? new List<ProductOption>();
        CreatedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;

    }

    // Constructor with full product details
    public CartItem(
        Guid cartId,
        Product product,
        int quantity,
        List<ProductOption>? options = null,
        decimal discountPercentage = 0,
        decimal taxRate = 0.10m)
    {
        if (product is null)
            throw new ArgumentNullException(nameof(product));

        Id = Guid.NewGuid();
        CartId = cartId;
        ProductId = product.Id;
        ProductName = product.Name;
        ProductSKU = product.SKU;
        ProductImage = product.Images?.FirstOrDefault();
        ProductImageThumbnail = product.ThumbnailUrl;
        ProductDescription = product.Description;
        ProductCategory = product.Category;


        Quantity = quantity;
        UnitPrice = product.Price;

        SelectedOptions = options ?? new List<ProductOption>();
        CreatedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;


    }

    // Update Methods
    public void UpdateQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero");

        if (quantity > 100)
            throw new DomainException("Maximum quantity per item is 100");

        Quantity = quantity;
     
        LastModifiedAt = DateTime.UtcNow;
    }

    public void UpdateUnitPrice(decimal unitPrice)
    {
        if (unitPrice < 0)
            throw new DomainException("Unit price cannot be negative");

        UnitPrice = unitPrice;

        LastModifiedAt = DateTime.UtcNow;
    }



    public void AddOption(string name, string value, decimal priceAdjustment = 0)
    {
        var option = new ProductOption
        {
            Name = name,
            Value = value,
            PriceAdjustment = priceAdjustment
        };

        SelectedOptions.Add(option);

        // Update unit price if option affects price
        if (priceAdjustment != 0)
        {
            UnitPrice += priceAdjustment;
     
        }

        LastModifiedAt = DateTime.UtcNow;
    }

    public void RemoveOption(string name)
    {
        var option = SelectedOptions.FirstOrDefault(o => o.Name == name);
        if (option != null)
        {
            SelectedOptions.Remove(option);

            // Recalculate unit price
            if (option.PriceAdjustment != 0)
            {
                UnitPrice -= option.PriceAdjustment;
             
            }

            LastModifiedAt = DateTime.UtcNow;
        }
    }

    public void UpdateProductImage(string imageUrl, string? thumbnailUrl = null)
    {
        ProductImage = imageUrl;
        ProductImageThumbnail = thumbnailUrl ?? imageUrl;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void UpdateProductSnapshot(Product product)
    {
        if (product is null)
            throw new ArgumentNullException(nameof(product));

        ProductName = product.Name;
        ProductSKU = product.SKU;
        ProductDescription = product.Description;
        ProductCategory = product.Category;


        LastModifiedAt = DateTime.UtcNow;
    }

    // Calculation Method





    // Calculated Properties
    public decimal Subtotal => UnitPrice * Quantity;
    public decimal LineTotal => TotalPrice;

    public bool HasOptions => SelectedOptions != null && SelectedOptions.Any();
    public string OptionsSummary => HasOptions
        ? string.Join(", ", SelectedOptions.Select(o => $"{o.Name}: {o.Value}"))
        : "None";

    public decimal GetItemTotalWithOptions()
    {
        var optionsTotal = SelectedOptions.Sum(o => o.PriceAdjustment);
        return (UnitPrice + optionsTotal) * Quantity;
    }

  

    public bool IsInStock()
    {
        return Product != null && Product.StockQuantity >= Quantity;
    }

    public int GetAvailableStock()
    {
        return Product?.StockQuantity ?? 0;
    }

    public bool IsAvailable()
    {
        return Product != null && Product.IsActive && Product.StockQuantity > 0;
    }

    // Validation Methods
    public bool ValidateStock()
    {
        if (Product == null)
            return false;

        return Product.StockQuantity >= Quantity;
    }

    public string GetValidationMessage()
    {
        if (Product == null)
            return "Product no longer available";

        if (!Product.IsActive)
            return $"Product '{ProductName}' is currently unavailable";

        if (Product.StockQuantity < Quantity)
            return $"Only {Product.StockQuantity} units of '{ProductName}' available";

        return string.Empty;
    }

    // Copy/Clone Method
    public CartItem Clone()
    {
        return new CartItem(
            cartId: CartId,
            productId: ProductId,
            productName: ProductName,
            quantity: Quantity,
            unitPrice: UnitPrice,
            productSKU: ProductSKU,
            productImage: ProductImage,
            options: SelectedOptions.Select(o => new ProductOption
            {
                Name = o.Name,
                Value = o.Value,
                PriceAdjustment = o.PriceAdjustment
            }).ToList()
   
        );
    }

    public override string ToString()
    {
        return $"{Quantity}x {ProductName} @ {UnitPrice:C} = {TotalPrice:C}";
    }

    public string GetFormattedDescription()
    {
        var description = $"{Quantity}x {ProductName}";
        if (HasOptions)
        {
            description += $" ({OptionsSummary})";
        }
        return description;
    }
}

public enum CartItemStatus
{
    Active,
    Saved,          // Saved for later
    OutOfStock,     // Product is no longer in stock
    PriceChanged,   // Product price has changed
    Removed         // Removed from cart
}