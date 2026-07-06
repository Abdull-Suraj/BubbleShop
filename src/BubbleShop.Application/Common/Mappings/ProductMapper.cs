using BubbleShop.Application.DTOs;
using BubbleShop.Domain.Entities;

namespace BubbleShop.Application.Common.Mappings;

public static class ProductMapper
{
    public static ProductDto ToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            ImageUrl = product.ImageUrl,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt,
            LastModifiedAt = product.LastModifiedAt
        };
    }

    public static IReadOnlyList<ProductDto> ToDtoList(IEnumerable<Product> products)
    {
        return products.Select(ToDto).ToList();
    }
}