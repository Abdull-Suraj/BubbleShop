
using BubbleShop.Domain.Entities;
using BubbleShop.Domain.Enums;

namespace BubbleShop.Application.DTOs;

/// <summary>
/// Ticket statistics DTO
/// </summary>
public class TicketStatistics
{
    public int TotalTickets { get; set; }
    public int OpenTickets { get; set; }
    public int InProgressTickets { get; set; }
    public int ResolvedTickets { get; set; }
    public int ClosedTickets { get; set; }
    public int UrgentTickets { get; set; }
    public int HighPriorityTickets { get; set; }
    public int NormalPriorityTickets { get; set; }
    public int LowPriorityTickets { get; set; }
    public int OverdueTickets { get; set; }
    public double AverageResponseTimeHours { get; set; }
    public double AverageResolutionTimeHours { get; set; }
    public double SatisfactionRate { get; set; }
    public Dictionary<TicketCategory, int> TicketsByCategory { get; set; } = new();
    public Dictionary<string, int> TicketsByChannel { get; set; } = new();
    public List<DailyTicketCount> Last7DaysTickets { get; set; } = new();
}
