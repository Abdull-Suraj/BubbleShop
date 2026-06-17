using BubbleShop.Domain.Entities;
using BubbleShop.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BubbleShop.Infrastructure.Persistence.Repositories;

public sealed class ProductRepository(AppDbContext dbContext) : Repository<Product>(dbContext), IProductRepository
{
    public async Task<IReadOnlyList<Product>> SearchAsync(string? keyword, string? category, CancellationToken cancellationToken = default)
    {
        var query = DbContext.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(x => x.Name.Contains(keyword) || x.Description.Contains(keyword));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(x => x.Category == category);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetAvailableAsync(CancellationToken cancellationToken = default)
        => await DbContext.Products.Where(x => x.IsActive && x.StockQuantity > 0).ToListAsync(cancellationToken);

    // NEW: Get products by business ID
    public async Task<IReadOnlyList<Product>> GetByBusinessIdAsync(Guid businessId, CancellationToken cancellationToken = default)
        => await DbContext.Products
            .Where(x => x.BusinessId == businessId && !x.IsDeleted)
            .ToListAsync(cancellationToken);

    // NEW: Get product by ID with includes
    public async Task<Product?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
        => await DbContext.Products
            .Include(x => x.Business)
            .Include(x => x.OrderItems)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

    // NEW: Get products by category
    public async Task<IReadOnlyList<Product>> GetByCategoryAsync(string category, Guid businessId, CancellationToken cancellationToken = default)
        => await DbContext.Products
            .Where(x => x.BusinessId == businessId && x.Category == category && x.IsActive && !x.IsDeleted)
            .ToListAsync(cancellationToken);

    // NEW: Get low stock products
    public async Task<IReadOnlyList<Product>> GetLowStockProductsAsync(Guid businessId, int threshold = 10, CancellationToken cancellationToken = default)
        => await DbContext.Products
            .Where(x => x.BusinessId == businessId && x.StockQuantity <= threshold && x.StockQuantity > 0 && !x.IsDeleted)
            .ToListAsync(cancellationToken);

    // NEW: Get out of stock products
    public async Task<IReadOnlyList<Product>> GetOutOfStockProductsAsync(Guid businessId, CancellationToken cancellationToken = default)
        => await DbContext.Products
            .Where(x => x.BusinessId == businessId && x.StockQuantity == 0 && !x.IsDeleted)
            .ToListAsync(cancellationToken);
}