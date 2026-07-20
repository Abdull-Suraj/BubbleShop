
using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Domain.Entities;
using BubbleShop.Domain.Enums;
using BubbleShop.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace BubbleShop.Application.AppServices;

public class AutomationService : IAutomationService
{
    private readonly IAutomationRuleRepository _automationRuleRepository;
    private readonly IProductRepository _productRepository;
    private readonly ILogger<AutomationService> _logger;

    public AutomationService(
        IAutomationRuleRepository automationRuleRepository,
        IProductRepository productRepository,
        ILogger<AutomationService> logger)
    {
        _automationRuleRepository = automationRuleRepository;
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<string?> ProcessAutomationAsync(string message, Guid businessId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing automation for business {BusinessId}: {Message}", businessId, message);

            var rules = await _automationRuleRepository.GetMatchingRulesAsync(message, businessId, cancellationToken);

            if (!rules.Any())
                return null;

            // Get the highest priority rule
            var rule = rules.OrderBy(r => r.Priority).First();

            // Increment trigger count
            rule.IncrementTriggerCount();
            await _automationRuleRepository.UpdateAsync(rule, cancellationToken);

            return await GenerateResponseAsync(rule, message, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing automation for business: {BusinessId}", businessId);
            return null;
        }
    }

    public async Task<bool> HasMatchingRuleAsync(string message, Guid businessId, CancellationToken cancellationToken = default)
    {
        var rules = await GetMatchingRulesAsync(message, businessId, cancellationToken);
        return rules.Any();
    }

    public async Task<IReadOnlyList<AutomationRule>> GetMatchingRulesAsync(string message, Guid businessId, CancellationToken cancellationToken = default)
    {
        return await _automationRuleRepository.GetMatchingRulesAsync(message, businessId, cancellationToken);
    }

    private async Task<string> GenerateResponseAsync(AutomationRule rule, string message, CancellationToken cancellationToken)
    {
        return rule.Action switch
        {
            RuleAction.ReplyWithMessage => rule.AutoReplyMessage,

            RuleAction.ShowProductDetails => await GetProductDetails(rule.AssociatedProductId, cancellationToken),

            RuleAction.InitiatePurchase => await InitiatePurchase(rule, cancellationToken),

            RuleAction.ProvidePricing => await GetProductPricing(rule.AssociatedProductId, cancellationToken),

            RuleAction.ShowCategory => await GetCategoryProducts(rule.AutoReplyMessage, cancellationToken),

            RuleAction.ShowAllProducts => await GetAllProducts(cancellationToken),

            RuleAction.ContactSupport => "I'll connect you with a support agent. Please wait...",

            _ => rule.AutoReplyMessage
        };
    }

    private async Task<string> GetProductDetails(Guid? productId, CancellationToken cancellationToken)
    {
        if (!productId.HasValue)
            return "Product not specified.";

        var product = await _productRepository.GetByIdAsync(productId.Value, cancellationToken);
        if (product == null)
            return "Product not found.";

        return $"**{product.Name}**\n" +
               $"{product.Description}\n" +
               $"Price: ${product.Price}\n" +
               $"Stock: {product.StockQuantity} units\n\n" +
               $"Reply 'buy {product.Name}' to purchase!";
    }

    private async Task<string> InitiatePurchase(AutomationRule rule, CancellationToken cancellationToken)
    {
        if (!rule.AssociatedProductId.HasValue)
            return "Product not specified for purchase.";

        var product = await _productRepository.GetByIdAsync(rule.AssociatedProductId.Value, cancellationToken);
        if (product == null)
            return "Product not found.";

        return $"Great choice! You're about to purchase **{product.Name}**.\n" +
               $"Price: ${product.Price}\n\n" +
               $"Reply 'confirm purchase' to proceed with payment.";
    }

    private async Task<string> GetProductPricing(Guid? productId, CancellationToken cancellationToken)
    {
        if (!productId.HasValue)
            return "Product not specified.";

        var product = await _productRepository.GetByIdAsync(productId.Value, cancellationToken);
        if (product == null)
            return "Product not found.";

        return $"**{product.Name}** costs ${product.Price}";
    }

    private async Task<string> GetCategoryProducts(string category, CancellationToken cancellationToken)
    {
        // This would fetch products by category
        return $"📋 Here are our products in category: **{category}**\n\n" +
               $"Would you like to see details of a specific product?";
    }

    private async Task<string> GetAllProducts(CancellationToken cancellationToken)
    {
        return "  Here are all our available products:\n\n" +
               "• Rice - Premium quality\n" +
               "• Beans - Fresh harvest\n" +
               "• Eggs - Farm fresh\n" +
               "• Meat - Well-packaged\n\n" +
               "Reply with product name to see details!";
    }
}