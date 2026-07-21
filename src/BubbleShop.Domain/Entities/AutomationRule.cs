using BubbleShop.Domain.Common;
using BubbleShop.Domain.Enums;
using RuleMatchType = BubbleShop.Domain.Enums.MatchType;
namespace BubbleShop.Domain.Entities;


public class AutomationRule : BaseEntity
{
    // Core Properties
    public Guid BusinessId { get; private set; }
    public string TriggerKeyword { get; private set; } = string.Empty;
    public string AutoReplyMessage { get; private set; } = string.Empty;
    public Guid? AssociatedProductId { get; private set; }
    public RuleAction Action { get; private set; }
    public bool IsActive { get; private set; }

    // Priority (for when multiple rules match)
    public int Priority { get; private set; } = 0;

    // Match Type (Exact, Contains, StartsWith, EndsWith)
    public RuleMatchType MatchType { get; private set; } = RuleMatchType.Contains;

    // Optional: Time restrictions
    public TimeOnly? StartTime { get; private set; }
    public TimeOnly? EndTime { get; private set; }
    public HashSet<DayOfWeek> ActiveDays { get; private set; } = new();

    // Usage tracking
    public int TimesTriggered { get; private set; }
    public DateTime? LastTriggeredAt { get; private set; }

    // Navigation Properties
    public Business Business { get; private set; } = null!;
    public Product? AssociatedProduct { get; private set; }

    private AutomationRule() { } // EF Core

    public AutomationRule(
        Guid businessId,
        string triggerKeyword,
        string autoReplyMessage,
        RuleAction action,
        Guid? associatedProductId = null,
        RuleMatchType matchType = RuleMatchType.Contains,
        int priority = 0)
    {
        Id = Guid.NewGuid();
        BusinessId = businessId;
        TriggerKeyword = triggerKeyword.ToLowerInvariant();
        AutoReplyMessage = autoReplyMessage;
        Action = action;
        AssociatedProductId = associatedProductId;
        MatchType = matchType;
        Priority = priority;
        IsActive = true;
        TimesTriggered = 0;
        ActiveDays = new HashSet<DayOfWeek>();
        CreatedAt = DateTime.UtcNow;
    }

    // Update Methods
    public void Update(
        string triggerKeyword,
        string autoReplyMessage,
        RuleAction action,
        Guid? associatedProductId = null,
        RuleMatchType matchType = RuleMatchType.Contains,
        int priority = 0)
    {
        TriggerKeyword = triggerKeyword.ToLowerInvariant();
        AutoReplyMessage = autoReplyMessage;
        Action = action;
        AssociatedProductId = associatedProductId;
        MatchType = matchType;
        Priority = priority;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void IncrementTriggerCount()
    {
        TimesTriggered++;
        LastTriggeredAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Reset trigger count for this rule
    /// </summary>
    public void ResetTriggerCount()
    {
        TimesTriggered = 0;
        LastTriggeredAt = null;
        LastModifiedAt = DateTime.UtcNow;
    }

    public bool IsMatch(string message)
    {
        if (!IsActive)
            return false;

        // Check time restrictions
        if (!IsTimeAllowed())
            return false;

        // Check day restrictions
        if (!IsDayAllowed())
            return false;

        var lowerMessage = message.ToLowerInvariant();

        return MatchType switch
        {
            RuleMatchType.Exact => lowerMessage == TriggerKeyword,
            RuleMatchType.Contains => lowerMessage.Contains(TriggerKeyword),
            RuleMatchType.StartsWith => lowerMessage.StartsWith(TriggerKeyword),
            RuleMatchType.EndsWith => lowerMessage.EndsWith(TriggerKeyword),
            RuleMatchType.Regex => System.Text.RegularExpressions.Regex.IsMatch(lowerMessage, TriggerKeyword),
            _ => lowerMessage.Contains(TriggerKeyword)
        };
    }

    private bool IsTimeAllowed()
    {
        if (!StartTime.HasValue || !EndTime.HasValue)
            return true;

        var now = TimeOnly.FromDateTime(DateTime.UtcNow);
        return now >= StartTime.Value && now <= EndTime.Value;
    }

    private bool IsDayAllowed()
    {
        if (!ActiveDays.Any())
            return true;

        var today = DateTime.UtcNow.DayOfWeek;
        return ActiveDays.Contains(today);
    }

    public void SetTimeRestrictions(TimeOnly? startTime, TimeOnly? endTime)
    {
        StartTime = startTime;
        EndTime = endTime;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void SetActiveDays(HashSet<DayOfWeek> days)
    {
        ActiveDays = days ?? new HashSet<DayOfWeek>();
        LastModifiedAt = DateTime.UtcNow;
    }

    public override string ToString()
    {
        return $"[{Action}] {TriggerKeyword} -> {AutoReplyMessage}";
    }
}

