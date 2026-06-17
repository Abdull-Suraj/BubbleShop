using BubbleShop.Domain.Entities;

namespace BubbleShop.Domain.Interfaces.Repositories;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<IReadOnlyList<Customer>> GetByBusinessIdAsync(Guid businessId, CancellationToken cancellationToken = default);
    Task<Customer?> GetByWhatsAppNumberAsync(string whatsappNumber, CancellationToken cancellationToken = default);
    Task<Customer?> GetByWhatsAppNumberAsync(string whatsappNumber, Guid businessId, CancellationToken cancellationToken = default);
    Task<Customer?> GetByEmailAsync(string email, Guid businessId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Customer>> GetActiveCustomersByBusinessIdAsync(Guid businessId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Customer>> SearchCustomersAsync(Guid businessId, string searchTerm, CancellationToken cancellationToken = default);
}