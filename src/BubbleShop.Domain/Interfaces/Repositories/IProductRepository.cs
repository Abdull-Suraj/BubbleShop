using BubbleShop.Domain.Entities;

namespace BubbleShop.Domain.Interfaces.Repositories;

public interface IProductRepository : IRepository<Product>
{
    Task<IReadOnlyList<Product>> SearchAsync(string? keyword, string? category, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetAvailableAsync(CancellationToken cancellationToken = default);
}
