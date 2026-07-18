using BubbleShop.Domain.Enums;

namespace BubbleShop.Domain.Models;

/// <summary>
/// Feedback statistics DTO
/// </summary>
public class FeedbackStatistics
{
    public int TotalFeedback { get; set; }
    public double AverageRating { get; set; }
    public int PositiveCount { get; set; } // Rating >= 4
    public int NeutralCount { get; set; }  // Rating == 3
    public int NegativeCount { get; set; } // Rating <= 2
    public Dictionary<int, int> RatingDistribution { get; set; } = new();
    public Dictionary<string, int> FeedbackByChannel { get; set; } = new();
    public Dictionary<TicketCategory, int> FeedbackByCategory { get; set; } = new();
    public List<RecentFeedbackDto> RecentFeedback { get; set; } = new();
    public FeedbackTrend Trend { get; set; } = new();
}
