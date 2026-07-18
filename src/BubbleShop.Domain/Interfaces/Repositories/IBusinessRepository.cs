// Domain/Interfaces/Repositories/IBusinessRepository.cs
using BubbleShop.Domain.Entities;
using BubbleShop.Domain.Enums;

namespace BubbleShop.Domain.Interfaces.Repositories;

public interface IBusinessRepository : IRepository<Business>
{
    Task<Business?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Business?> GetByWhatsAppNumberAsync(string whatsAppNumber, CancellationToken cancellationToken = default);
    Task<Business?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default);
    Task<Business?> GetByRegistrationNumberAsync(string registrationNumber, CancellationToken cancellationToken = default);
    Task<Business?> GetByTaxIdAsync(string taxId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ExistsByWhatsAppAsync(string whatsAppNumber, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Business>> GetActiveBusinessesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Business>> GetBusinessesByStatusAsync(BusinessStatus status, CancellationToken cancellationToken = default);
   
    Task<IReadOnlyList<Business>> SearchBusinessesAsync(string searchTerm, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<int> GetTotalCountAsync(BusinessStatus? status = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Business>> GetBusinessesPagedAsync(int page, int pageSize, string sortBy = "CreatedAt", bool descending = true, CancellationToken cancellationToken = default);
}