namespace BubbleShop.Domain.Models;

public class DailyFeedbackCount
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
    public double AverageRating { get; set; }
}
