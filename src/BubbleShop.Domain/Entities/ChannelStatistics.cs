namespace BubbleShop.Domain.Entities;

public class ChannelStatistics
{
    public int TotalMessagesReceived { get; set; }
    public int TotalMessagesSent { get; set; }
    public int TotalConversations { get; set; }
    public double AverageResponseTime { get; set; }
}