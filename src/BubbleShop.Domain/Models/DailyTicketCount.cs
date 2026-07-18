// Domain/Interfaces/Repositories/DailyTicketCount.cs
namespace BubbleShop.Application.DTOs;

public class DailyTicketCount
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
}