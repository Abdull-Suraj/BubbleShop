namespace BubbleShop.Domain.Models;

public class FeedbackTrend
{
    public double CurrentMonthAverage { get; set; }

    public double PreviousMonthAverage { get; set; }

    public double ChangePercentage { get; set; }
    public List<DailyFeedbackCount> Last7Days { get; set; } = new();
}
