// Infrastructure/Persistence/Repositories/BusinessRepository.cs
using Microsoft.EntityFrameworkCore;
using BubbleShop.Domain.Entities;
using BubbleShop.Domain.Enums;
using BubbleShop.Domain.Interfaces.Repositories;
using BubbleShop.Infrastructure.Persistence;

namespace BubbleShop.Infrastructure.Persistence.Repositories;

public class BusinessRepository : Repository<Business>, IBusinessRepository
{
    public BusinessRepository(AppDbContext context) : base(context)
    {
    }

    #region Get By Methods

    public async Task<Business?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(b => b.Email.ToLower() == email.ToLower() && !b.IsDeleted, cancellationToken);
    }

    public async Task<Business?> GetByWhatsAppNumberAsync(string whatsAppNumber, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(b => b.WhatsAppNumber == whatsAppNumber && !b.IsDeleted, cancellationToken);
    }

    public async Task<Business?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(b => b.PhoneNumber == phoneNumber && !b.IsDeleted, cancellationToken);
    }

    public async Task<Business?> GetByRegistrationNumberAsync(string registrationNumber, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(b => b.RegistrationNumber == registrationNumber && !b.IsDeleted, cancellationToken);
    }

    public async Task<Business?> GetByTaxIdAsync(string taxId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(b => b.TaxId == taxId && !b.IsDeleted, cancellationToken);
    }

    #endregion

    #region Exists Methods

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(b => b.Email.ToLower() == email.ToLower() && !b.IsDeleted, cancellationToken);
    }

    public async Task<bool> ExistsByWhatsAppAsync(string whatsAppNumber, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(b => b.WhatsAppNumber == whatsAppNumber && !b.IsDeleted, cancellationToken);
    }

    #endregion

    #region Get All / Filter Methods

    public async Task<IReadOnlyList<Business>> GetActiveBusinessesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(b => b.Status == BusinessStatus.Active && !b.IsDeleted)
            .OrderBy(b => b.BusinessName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Business>> GetBusinessesByStatusAsync(BusinessStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(b => b.Status == status && !b.IsDeleted)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);
    }
    #endregion

    #region Search & Pagination

    public async Task<IReadOnlyList<Business>> SearchBusinessesAsync(string searchTerm, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await GetBusinessesPagedAsync(page, pageSize, cancellationToken: cancellationToken);
        }

        var lowerSearchTerm = searchTerm.ToLower();

        return await _dbSet
            .Where(b => !b.IsDeleted &&
                (b.BusinessName.ToLower().Contains(lowerSearchTerm) ||
                 b.Email.ToLower().Contains(lowerSearchTerm) ||
                 b.WhatsAppNumber.Contains(searchTerm) ||
                 b.PhoneNumber.Contains(searchTerm) ||
                 b.City.ToLower().Contains(lowerSearchTerm) ||
                 b.State.ToLower().Contains(lowerSearchTerm) ||
                 b.LegalName.ToLower().Contains(lowerSearchTerm)))
            .OrderBy(b => b.BusinessName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetTotalCountAsync(BusinessStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(b => !b.IsDeleted);

        if (status.HasValue)
        {
            query = query.Where(b => b.Status == status.Value);
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Business>> GetBusinessesPagedAsync(int page, int pageSize, string sortBy = "CreatedAt", bool descending = true, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(b => !b.IsDeleted);

        // Apply sorting
        query = sortBy?.ToLower() switch
        {
            "businessname" => descending
                ? query.OrderByDescending(b => b.BusinessName)
                : query.OrderBy(b => b.BusinessName),
            "email" => descending
                ? query.OrderByDescending(b => b.Email)
                : query.OrderBy(b => b.Email),
            "status" => descending
                ? query.OrderByDescending(b => b.Status)
                : query.OrderBy(b => b.Status),
            "createdat" => descending
                ? query.OrderByDescending(b => b.CreatedAt)
                : query.OrderBy(b => b.CreatedAt),
            _ => descending
                ? query.OrderByDescending(b => b.CreatedAt)
                : query.OrderBy(b => b.CreatedAt)
        };

        query = query.Skip((page - 1) * pageSize).Take(pageSize);

        return await query.ToListAsync(cancellationToken);
    }

    #endregion
}