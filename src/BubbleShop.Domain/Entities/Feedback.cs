// Domain/Entities/Feedback.cs
using BubbleShop.Domain.Common;
using BubbleShop.Domain.Enums;

namespace BubbleShop.Domain.Entities;


public class Feedback : BaseEntity
{
    public Guid CustomerId { get; private set; }
    public Guid BusinessId { get; private set; }
    public Guid? OrderId { get; private set; }
    public int Rating { get; private set; } // 1-5
    public string? Comment { get; private set; }
    public string Channel { get; private set; } = string.Empty;
    public TicketCategory Category { get; private set; }
    public bool IsPublic { get; private set; }
    public bool IsAnonymous { get; private set; }
    public string? Response { get; private set; }
    public DateTime? RespondedAt { get; private set; }
    public List<string> Tags { get; private set; } = new();
    public Dictionary<string, string> Metadata { get; private set; } = new();

    // Navigation Properties
    public Customer Customer { get; private set; } = null!;
    public Business Business { get; private set; } = null!;
    public Order? Order { get; private set; }

    private Feedback() { }

    public Feedback(
        Guid customerId,
        Guid businessId,
        int rating,
        string? comment = null,
        string? channel = null,
        TicketCategory category = TicketCategory.Feedback,
        bool isPublic = true,
        bool isAnonymous = false)
    {
        Id = Guid.NewGuid();
        CustomerId = customerId;
        BusinessId = businessId;
        Rating = rating;
        Comment = comment;
        Channel = channel ?? "Unknown";
        Category = category;
        IsPublic = isPublic;
        IsAnonymous = isAnonymous;
        CreatedAt = DateTime.UtcNow;
        Tags = new List<string>();
        Metadata = new Dictionary<string, string>();
    }

    public void Respond(string response)
    {
        Response = response;
        RespondedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void AddTag(string tag)
    {
        if (!Tags.Contains(tag))
        {
            Tags.Add(tag);
            LastModifiedAt = DateTime.UtcNow;
        }
    }

    public void RemoveTag(string tag)
    {
        Tags.Remove(tag);
        LastModifiedAt = DateTime.UtcNow;
    }

    public void AddMetadata(string key, string value)
    {
        Metadata[key] = value;
        LastModifiedAt = DateTime.UtcNow;
    }

    public bool IsPositive => Rating >= 4;
    public bool IsNeutral => Rating == 3;
    public bool IsNegative => Rating <= 2;
    public string RatingEmoji => Rating >= 4 ? "😊" : Rating == 3 ? "😐" : "😞";

    public string GetRatingLabel()
    {
        return Rating switch
        {
            5 => "Excellent",
            4 => "Good",
            3 => "Average",
            2 => "Poor",
            1 => "Terrible",
            _ => "Unknown"
        };
    }

    public override string ToString()
    {
        return $"{RatingEmoji} {Rating}/5 - {GetRatingLabel()}";
    }
}