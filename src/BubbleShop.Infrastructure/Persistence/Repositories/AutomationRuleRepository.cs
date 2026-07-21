using Microsoft.EntityFrameworkCore;
using BubbleShop.Domain.Entities;
using BubbleShop.Domain.Interfaces.Repositories;
using BubbleShop.Infrastructure.Persistence;
using BubbleShop.Domain.Enums;

namespace BubbleShop.Infrastructure.Persistence.Repositories;

public class AutomationRuleRepository : Repository<AutomationRule>, IAutomationRuleRepository
{
    public AutomationRuleRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<AutomationRule>> GetActiveRulesByBusinessIdAsync(Guid businessId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.BusinessId == businessId && r.IsActive && !r.IsDeleted)
            .OrderBy(r => r.Priority)
            .ToListAsync(cancellationToken);
    }

    public async Task<AutomationRule?> GetByTriggerKeywordAsync(string keyword, Guid businessId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(r => r.TriggerKeyword == keyword.ToLowerInvariant()
                                   && r.BusinessId == businessId
                                   && !r.IsDeleted, cancellationToken);
    }

    public async Task<IReadOnlyList<AutomationRule>> GetRulesByActionAsync(RuleAction action, Guid businessId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.Action == action && r.BusinessId == businessId && r.IsActive && !r.IsDeleted)
            .OrderBy(r => r.Priority)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AutomationRule>> GetRulesForProductAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.AssociatedProductId == productId && r.IsActive && !r.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AutomationRule>> GetMatchingRulesAsync(string message, Guid businessId, CancellationToken cancellationToken = default)
    {
        var rules = await GetActiveRulesByBusinessIdAsync(businessId, cancellationToken);
        return rules
            .Where(r => r.IsMatch(message))
            .OrderBy(r => r.Priority)
            .ToList();
    }

    public async Task<AutomationRule?> GetHighestPriorityRuleAsync(string message, Guid businessId, CancellationToken cancellationToken = default)
    {
        var matchingRules = await GetMatchingRulesAsync(message, businessId, cancellationToken);
        return matchingRules.FirstOrDefault();
    }
}