// Domain/Entities/SupportTicket.cs
using BubbleShop.Domain.Common;
using BubbleShop.Domain.Enums;
using BubbleShop.Domain.Exceptions;


namespace BubbleShop.Domain.Entities;

/// <summary>
/// Support ticket entity for customer support
/// </summary>
public class SupportTicket : BaseEntity
{
    // Core Identifiers
    public Guid CustomerId { get; private set; }
    public Guid BusinessId { get; private set; }

    // Ticket Information
    public string TicketNumber { get; private set; } = string.Empty;
    public string Subject { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;

    // Status and Priority
    public TicketStatus Status { get; private set; }
    public TicketPriority Priority { get; private set; }
    public TicketCategory Category { get; private set; }

    // Channel Information
    public string Channel { get; private set; } = string.Empty; // WhatsApp, Telegram, Web, Email, etc.
    public string? ChannelConversationId { get; private set; }

    // Assignment
    public Guid? AssignedToAgentId { get; private set; }
    public string? AssignedToAgentName { get; private set; }
    public DateTime? AssignedAt { get; private set; }

    // Resolution
    public string? Resolution { get; private set; }
    public DateTime? ResolvedAt { get; private set; }
    public DateTime? ClosedAt { get; private set; }

    // Customer Satisfaction
    public int? SatisfactionRating { get; private set; } // 1-5
    public string? SatisfactionComment { get; private set; }

    // Timestamps
    public DateTime? LastActivityAt { get; private set; }
    public DateTime? FirstResponseAt { get; private set; }
    public int ResponseCount { get; private set; }

    // Additional Data
    public Dictionary<string, string> Metadata { get; private set; } = new();
    public List<TicketComment> Comments { get; private set; } = new();

    // Navigation Properties
    public Customer Customer { get; private set; } = null!;
    public Business Business { get; private set; } = null!;
    public User? AssignedToAgent { get; private set; }

    private SupportTicket() { }

    public SupportTicket(
        Guid customerId,
        Guid businessId,
        string subject,
        string message,
        string channel,
        TicketCategory category = TicketCategory.General,
        TicketPriority priority = TicketPriority.Normal)
    {
        Id = Guid.NewGuid();
        TicketNumber = GenerateTicketNumber();
        CustomerId = customerId;
        BusinessId = businessId;
        Subject = subject ?? throw new ArgumentNullException(nameof(subject));
        Message = message ?? throw new ArgumentNullException(nameof(message));
        Channel = channel ?? "Unknown";
        Category = category;
        Priority = priority;
        Status = TicketStatus.Open;
        CreatedAt = DateTime.UtcNow;
        LastActivityAt = DateTime.UtcNow;
        ResponseCount = 0;
        Metadata = new Dictionary<string, string>();
        Comments = new List<TicketComment>();
    }

    public void AssignToAgent(Guid agentId, string agentName)
    {
        if (Status == TicketStatus.Closed || Status == TicketStatus.Resolved)
            throw new DomainException("Cannot assign a closed or resolved ticket");

        AssignedToAgentId = agentId;
        AssignedToAgentName = agentName;
        AssignedAt = DateTime.UtcNow;
        Status = TicketStatus.InProgress;
        LastActivityAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void AddComment(string message, string author, bool isInternal = false)
    {
        if (Status == TicketStatus.Closed)
            throw new DomainException("Cannot add comment to closed ticket");

        Comments.Add(new TicketComment
        {
            Id = Guid.NewGuid(),
            Message = message,
            Author = author,
            IsInternal = isInternal,
            CreatedAt = DateTime.UtcNow
        });

        ResponseCount++;
        LastActivityAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;

        if (FirstResponseAt is null)
        {
            FirstResponseAt = DateTime.UtcNow;
        }
    }

    public void AddCustomerResponse(string message, string customerName)
    {
        AddComment(message, customerName, false);
        Status = TicketStatus.WaitingForAgent;
        LastActivityAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void AddAgentResponse(string message, string agentName)
    {
        AddComment(message, agentName, false);
        Status = TicketStatus.InProgress;
        LastActivityAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void MarkAsResolved(string resolution)
    {
        if (Status == TicketStatus.Closed)
            throw new DomainException("Ticket is already closed");

        Resolution = resolution;
        ResolvedAt = DateTime.UtcNow;
        Status = TicketStatus.Resolved;
        LastActivityAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Close()
    {
        if (Status == TicketStatus.Closed)
            throw new DomainException("Ticket is already closed");

        if (Status != TicketStatus.Resolved)
        {
            Status = TicketStatus.Resolved;
            ResolvedAt = DateTime.UtcNow;
        }

        Status = TicketStatus.Closed;
        ClosedAt = DateTime.UtcNow;
        LastActivityAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Reopen()
    {
        if (Status != TicketStatus.Closed && Status != TicketStatus.Resolved)
            throw new DomainException("Only closed or resolved tickets can be reopened");

        Status = TicketStatus.Open;
        ClosedAt = null;
        ResolvedAt = null;
        LastActivityAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void UpdatePriority(TicketPriority priority)
    {
        Priority = priority;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void UpdateCategory(TicketCategory category)
    {
        Category = category;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void AddSatisfactionRating(int rating, string? comment = null)
    {
        if (rating < 1 || rating > 5)
            throw new DomainException("Rating must be between 1 and 5");

        if (Status != TicketStatus.Closed && Status != TicketStatus.Resolved)
            throw new DomainException("Only closed or resolved tickets can be rated");

        SatisfactionRating = rating;
        SatisfactionComment = comment;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void AddMetadata(string key, string value)
    {
        Metadata[key] = value;
        LastModifiedAt = DateTime.UtcNow;
    }

    public string GetStatusDisplay()
    {
        return Status switch
        {
            TicketStatus.Open => "🟢 Open",
            TicketStatus.InProgress => "🔄 In Progress",
            TicketStatus.WaitingForAgent => "⏳ Waiting for Agent",
            TicketStatus.WaitingForCustomer => "⏳ Waiting for Customer",
            TicketStatus.Resolved => "✅ Resolved",
            TicketStatus.Closed => "🔒 Closed",
            _ => "Unknown"
        };
    }

    public string GetPriorityDisplay()
    {
        return Priority switch
        {
            TicketPriority.Low => "🟢 Low",
            TicketPriority.Normal => "🔵 Normal",
            TicketPriority.High => "🟠 High",
            TicketPriority.Urgent => "🔴 Urgent",
            _ => "Unknown"
        };
    }

    public TimeSpan GetResponseTime()
    {
        if (!FirstResponseAt.HasValue)
            return TimeSpan.Zero;

        return FirstResponseAt.Value - CreatedAt;
    }

    public TimeSpan GetResolutionTime()
    {
        if (!ResolvedAt.HasValue && !ClosedAt.HasValue)
            return TimeSpan.Zero;

        var endTime = ResolvedAt ?? ClosedAt ?? DateTime.UtcNow;
        return endTime - CreatedAt;
    }

    public bool IsOverdue()
    {
        if (Status == TicketStatus.Closed || Status == TicketStatus.Resolved)
            return false;

        var overdueThreshold = Priority switch
        {
            TicketPriority.Urgent => TimeSpan.FromHours(4),
            TicketPriority.High => TimeSpan.FromHours(8),
            TicketPriority.Normal => TimeSpan.FromHours(24),
            TicketPriority.Low => TimeSpan.FromHours(48),
            _ => TimeSpan.FromHours(24)
        };

        return DateTime.UtcNow - LastActivityAt > overdueThreshold;
    }

    private static string GenerateTicketNumber()
    {
        return $"TKT-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid():N}"[..8].ToUpper();
    }

    public override string ToString()
    {
        return $"#{TicketNumber} - {Subject} ({Status})";
    }
}
