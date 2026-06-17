
using BubbleShop.Domain.Entities;

namespace BubbleShop.Application.Common.Interfaces;

public interface IAutomationService
{
    /// <summary>
    /// Process an incoming message through automation rules
    /// </summary>
    /// <param name="message">The incoming message</param>
    /// <param name="businessId">The business ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The automated response, or null if no rule matches</returns>
    Task<string?> ProcessAutomationAsync(string message, Guid businessId, CancellationToken cancellationToken = default);


    Task<bool> HasMatchingRuleAsync(string message, Guid businessId, CancellationToken cancellationToken = default);


    Task<IReadOnlyList<AutomationRule>> GetMatchingRulesAsync(string message, Guid businessId, CancellationToken cancellationToken = default);
}