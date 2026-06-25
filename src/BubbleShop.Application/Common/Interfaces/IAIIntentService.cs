// Application/Common/Interfaces/IAIIntentService.cs
using BubbleShop.Application.Common.Models;
using BubbleShop.Domain.Common;

namespace BubbleShop.Application.Common.Interfaces;

public interface IAIIntentService
{
    Task<IntentResult> AnalyzeIntentAsync(string message, MessageContext context, CancellationToken cancellationToken = default);
    Task<string> GenerateResponseAsync(IntentResult intent, MessageContext context, CancellationToken cancellationToken = default);
}