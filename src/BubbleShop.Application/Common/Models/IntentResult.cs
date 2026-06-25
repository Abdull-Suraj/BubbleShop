// Application/Common/Models/IntentResult.cs
using BubbleShop.Domain.Common;

namespace BubbleShop.Application.Common.Models;

public class IntentResult
{
    public Intent Intent { get; set; }
    public decimal Confidence { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
    public string RawMessage { get; set; } = string.Empty;
    public List<string> ExtractedEntities { get; set; } = new();
    public string ResponseMessage { get; set; } = string.Empty;
    public bool RequiresConfirmation { get; set; }
    public List<string> SuggestedResponses { get; set; } = new();
}
