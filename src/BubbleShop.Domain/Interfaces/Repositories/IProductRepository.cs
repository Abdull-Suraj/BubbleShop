// Domain/Interfaces/Repositories/IProductRepository.cs
using BubbleShop.Domain.Entities;

namespace BubbleShop.Domain.Interfaces.Repositories;

public interface IProductRepository : IRepository<Product>
{
    Task<IReadOnlyList<Product>> SearchAsync(string? keyword, string? category, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetAvailableAsync(CancellationToken cancellationToken = default);

    // Add these methods
    Task<IReadOnlyList<Product>> GetByBusinessIdAsync(Guid businessId, CancellationToken cancellationToken = default);
    Task<Product?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetByCategoryAsync(string category, Guid businessId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetLowStockProductsAsync(Guid businessId, int threshold = 10, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetOutOfStockProductsAsync(Guid businessId, CancellationToken cancellationToken = default);
}