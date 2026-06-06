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

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetAvailableAsync(CancellationToken cancellationToken = default)
        => await DbContext.Products.Where(x => x.IsActive && x.StockQuantity > 0).ToListAsync(cancellationToken);
}
