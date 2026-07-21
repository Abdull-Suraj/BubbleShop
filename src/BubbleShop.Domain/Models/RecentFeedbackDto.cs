namespace BubbleShop.Domain.Models;

public class RecentFeedbackDto
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public string Channel { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
