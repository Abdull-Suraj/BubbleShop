namespace BubbleShop.Domain.Models;

public class FeedbackResponseStats
{
    public int TotalResponded { get; set; }
    public int TotalUnresponded { get; set; }
    public double AverageResponseTimeHours { get; set; }
    public double MaxResponseTimeHours { get; set; }
    public double MinResponseTimeHours { get; set; }
}