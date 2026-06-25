// Application/Services/AIIntentService.cs
using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Application.Common.Models;
using BubbleShop.Domain.Common;
using Microsoft.Extensions.Logging;
using Intent = BubbleShop.Application.Common.Models.Intent;

namespace BubbleShop.Application.AppServices;

public class AIIntentService : IAIIntentService
{
    private readonly ILogger<AIIntentService> _logger;

    public AIIntentService(ILogger<AIIntentService> logger)
    {
        _logger = logger;
    }

    public async Task<IntentResult> AnalyzeIntentAsync(string message, MessageContext context, CancellationToken cancellationToken = default)
    {
        var lowerMessage = message.ToLower();
        var result = new IntentResult
        {
            RawMessage = message,
            Parameters = new Dictionary<string, object>(),
            ExtractedEntities = new List<string>(),
            Confidence = 0.8m
        };

        if (lowerMessage.Contains("want") || lowerMessage.Contains("buy") || lowerMessage.Contains("order"))
        {
            result.Intent = Intent.CreateOrder;
            result.ResponseMessage = "I'll help you create an order! What would you like to buy?";
        }
        else if (lowerMessage.Contains("price") || lowerMessage.Contains("how much"))
        {
            result.Intent = Intent.GetProductPrice;
            result.ResponseMessage = "Let me check the price for you.";
        }
        else if (lowerMessage.Contains("track") || lowerMessage.Contains("where"))
        {
            result.Intent = Intent.TrackOrder;
            result.ResponseMessage = "I'll help you track your order.";
        }
        else if (lowerMessage.Contains("help") || lowerMessage.Contains("?"))
        {
            result.Intent = Intent.GetHelp;
            result.ResponseMessage = "How can I help you today?";
        }
        else
        {
            result.Intent = Intent.JustChatting;
            result.ResponseMessage = "Hello! How can I assist you?";
        }

        return await Task.FromResult(result);
    }

    public async Task<string> GenerateResponseAsync(IntentResult intent, MessageContext context, CancellationToken cancellationToken = default)
    {
        return intent.ResponseMessage ?? "How can I help you?";
    }
}