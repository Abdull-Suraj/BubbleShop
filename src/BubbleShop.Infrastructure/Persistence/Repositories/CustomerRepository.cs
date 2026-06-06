using BubbleShop.Domain.Entities;
using BubbleShop.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BubbleShop.Infrastructure.Persistence.Repositories;

public sealed class CustomerRepository(AppDbContext dbContext) : Repository<Customer>(dbContext), ICustomerRepository
{
    public async Task<Customer?> GetByWhatsAppNumberAsync(string whatsappNumber, CancellationToken cancellationToken = default)
        => await DbContext.Customers.FirstOrDefaultAsync(x => x.WhatsAppNumber == whatsappNumber, cancellationToken);
}
