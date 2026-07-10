using BubbleShop.Domain.Entities;
using BubbleShop.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BubbleShop.Infrastructure.Persistence.Repositories;

public sealed class CustomerRepository(AppDbContext dbContext) : Repository<Customer>(dbContext), ICustomerRepository
{
    public async Task<Customer?> GetByWhatsAppNumberAsync(string whatsappNumber, CancellationToken cancellationToken = default)
        => await DbContext.Customers
            .FirstOrDefaultAsync(x => x.WhatsAppNumber == whatsappNumber && !x.IsDeleted, cancellationToken);

    public async Task<Customer?> GetByWhatsAppNumberAsync(string whatsappNumber, Guid businessId, CancellationToken cancellationToken = default)
        => await DbContext.Customers
            .FirstOrDefaultAsync(x => x.WhatsAppNumber == whatsappNumber
                                   && x.BusinessId == businessId
                                   && !x.IsDeleted, cancellationToken);

    public async Task<Customer?> GetByEmailAsync(string email, Guid businessId, CancellationToken cancellationToken = default)
        => await DbContext.Customers
            .FirstOrDefaultAsync(x => x.Email == email
                                   && x.BusinessId == businessId
                                   && !x.IsDeleted, cancellationToken);

    public async Task<IReadOnlyList<Customer>> GetByBusinessIdAsync(Guid businessId, CancellationToken cancellationToken = default)
        => await DbContext.Customers
            .Where(x => x.BusinessId == businessId && !x.IsDeleted)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Customer>> GetActiveCustomersByBusinessIdAsync(Guid businessId, CancellationToken cancellationToken = default)
        => await DbContext.Customers
            .Where(x => x.BusinessId == businessId && x.Status == CustomerStatus.Active && !x.IsDeleted)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Customer>> SearchCustomersAsync(Guid businessId, string searchTerm, CancellationToken cancellationToken = default)
        => await DbContext.Customers
            .Where(x => x.BusinessId == businessId
         && !x.IsDeleted
         && (
             x.Name.Contains(searchTerm) ||
             x.WhatsAppNumber.Contains(searchTerm) ||
             (x.Email != null && x.Email.Contains(searchTerm))
         ))
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
}