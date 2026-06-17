
using BubbleShop.Domain.Entities;
using BubbleShop.Domain.Interfaces.Repositories;

public interface IBusinessRepository : IRepository<Business>
{
    Task<Business?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Business?> GetByWhatsAppNumberAsync(string whatsAppNumber, CancellationToken cancellationToken = default);
    // ... other methods
}