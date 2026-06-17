using BubbleShop.Domain.Entities;

namespace BubbleShop.Domain.Interfaces.Repositories;

public interface IAutomationRuleRepository : IRepository<AutomationRule>
{
    Task<IReadOnlyList<AutomationRule>> GetActiveRulesByBusinessIdAsync(Guid businessId, CancellationToken cancellationToken = default);
    Task<AutomationRule?> GetByTriggerKeywordAsync(string keyword, Guid businessId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AutomationRule>> GetRulesByActionAsync(RuleAction action, Guid businessId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AutomationRule>> GetRulesForProductAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AutomationRule>> GetMatchingRulesAsync(string message, Guid businessId, CancellationToken cancellationToken = default);
    Task<AutomationRule?> GetHighestPriorityRuleAsync(string message, Guid businessId, CancellationToken cancellationToken = default);
}