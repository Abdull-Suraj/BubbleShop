
using Microsoft.EntityFrameworkCore;
using BubbleShop.Domain.Entities;

using BubbleShop.Application.DTOs;
using BubbleShop.Domain.Interfaces.Repositories;
using BubbleShop.Domain.Models;
using BubbleShop.Domain.Enums;

namespace BubbleShop.Infrastructure.Persistence.Repositories;

public class SupportTicketRepository : Repository<SupportTicket>, ISupportTicketRepository
{
    public SupportTicketRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<SupportTicket?> GetByTicketNumberAsync(string ticketNumber, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.Customer)
            .Include(t => t.Business)
            .Include(t => t.Comments)
            .FirstOrDefaultAsync(t => t.TicketNumber == ticketNumber && !t.IsDeleted, cancellationToken);
    }

    public async Task<IReadOnlyList<SupportTicket>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.Comments)
            .Where(t => t.CustomerId == customerId && !t.IsDeleted)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SupportTicket>> GetByBusinessIdAsync(Guid businessId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.Customer)
            .Include(t => t.Comments)
            .Where(t => t.BusinessId == businessId && !t.IsDeleted)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SupportTicket>> GetByStatusAsync(TicketStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.Customer)
            .Where(t => t.Status == status && !t.IsDeleted)
            .OrderByDescending(t => t.Priority)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SupportTicket>> GetByPriorityAsync(TicketPriority priority, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.Customer)
            .Where(t => t.Priority == priority && !t.IsDeleted)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SupportTicket>> GetByCategoryAsync(TicketCategory category, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.Category == category && !t.IsDeleted)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SupportTicket>> GetByAssignedAgentAsync(Guid agentId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.Customer)
            .Where(t => t.AssignedToAgentId == agentId && !t.IsDeleted)
            .OrderByDescending(t => t.Priority)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SupportTicket>> GetOpenTicketsByBusinessIdAsync(Guid businessId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.Customer)
            .Where(t => t.BusinessId == businessId
                        && (t.Status == TicketStatus.Open
                            || t.Status == TicketStatus.InProgress
                            || t.Status == TicketStatus.WaitingForAgent
                            || t.Status == TicketStatus.WaitingForCustomer)
                        && !t.IsDeleted)
            .OrderByDescending(t => t.Priority)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SupportTicket>> GetOverdueTicketsAsync(CancellationToken cancellationToken = default)
    {
        var tickets = await _dbSet
            .Include(t => t.Customer)
            .Include(t => t.Business)
            .Where(t => !t.IsDeleted &&
                        t.Status != TicketStatus.Closed &&
                        t.Status != TicketStatus.Resolved)
            .ToListAsync(cancellationToken);

        return tickets.Where(t => t.IsOverdue()).ToList();
    }

    public async Task<IReadOnlyList<SupportTicket>> GetTicketsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.Customer)
            .Where(t => t.CreatedAt >= startDate && t.CreatedAt <= endDate && !t.IsDeleted)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SupportTicket>> GetTicketsByChannelAsync(string channel, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.Channel == channel && !t.IsDeleted)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<TicketStatus, int>> GetTicketCountsByStatusAsync(Guid businessId, CancellationToken cancellationToken = default)
    {
        var tickets = await _dbSet
            .Where(t => t.BusinessId == businessId && !t.IsDeleted)
            .ToListAsync(cancellationToken);

        return tickets
            .GroupBy(t => t.Status)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<TimeSpan> GetAverageResponseTimeAsync(Guid businessId, CancellationToken cancellationToken = default)
    {
        var tickets = await _dbSet
            .Where(t => t.BusinessId == businessId && t.FirstResponseAt.HasValue && !t.IsDeleted)
            .ToListAsync(cancellationToken);

        if (!tickets.Any())
            return TimeSpan.Zero;

        var totalResponseTime = tickets.Sum(t => t.GetResponseTime().TotalSeconds);
        var averageSeconds = totalResponseTime / tickets.Count;

        return TimeSpan.FromSeconds(averageSeconds);
    }

    public async Task<TimeSpan> GetAverageResolutionTimeAsync(Guid businessId, CancellationToken cancellationToken = default)
    {
        var tickets = await _dbSet
            .Where(t => t.BusinessId == businessId
                        && (t.ResolvedAt.HasValue || t.ClosedAt.HasValue)
                        && !t.IsDeleted)
            .ToListAsync(cancellationToken);

        if (!tickets.Any())
            return TimeSpan.Zero;

        var totalResolutionTime = tickets.Sum(t => t.GetResolutionTime().TotalSeconds);
        var averageSeconds = totalResolutionTime / tickets.Count;

        return TimeSpan.FromSeconds(averageSeconds);
    }

    public async Task<IReadOnlyList<SupportTicket>> SearchTicketsAsync(string keyword, Guid? businessId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(t => t.Customer)
            .Where(t => !t.IsDeleted);

        if (businessId.HasValue)
        {
            query = query.Where(t => t.BusinessId == businessId.Value);
        }

        var lowerKeyword = keyword.ToLower();

        return await query
            .Where(t => t.Subject.ToLower().Contains(lowerKeyword) ||
                        t.Message.ToLower().Contains(lowerKeyword) ||
                        t.TicketNumber.ToLower().Contains(lowerKeyword) ||
                        t.Comments.Any(c => c.Message.ToLower().Contains(lowerKeyword)))
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<TicketStatistics> GetTicketStatisticsAsync(Guid businessId, CancellationToken cancellationToken = default)
    {
        var tickets = await _dbSet
            .Include(t => t.Customer)
            .Where(t => t.BusinessId == businessId && !t.IsDeleted)
            .ToListAsync(cancellationToken);

        var statistics = new TicketStatistics
        {
            TotalTickets = tickets.Count,
            OpenTickets = tickets.Count(t => t.Status == TicketStatus.Open),
            InProgressTickets = tickets.Count(t => t.Status == TicketStatus.InProgress),
            ResolvedTickets = tickets.Count(t => t.Status == TicketStatus.Resolved),
            ClosedTickets = tickets.Count(t => t.Status == TicketStatus.Closed),
            UrgentTickets = tickets.Count(t => t.Priority == TicketPriority.Urgent),
            HighPriorityTickets = tickets.Count(t => t.Priority == TicketPriority.High),
            NormalPriorityTickets = tickets.Count(t => t.Priority == TicketPriority.Normal),
            LowPriorityTickets = tickets.Count(t => t.Priority == TicketPriority.Low),
            OverdueTickets = tickets.Count(t => t.IsOverdue()),
            TicketsByCategory = tickets.GroupBy(t => t.Category).ToDictionary(g => g.Key, g => g.Count()),
            TicketsByChannel = tickets.GroupBy(t => t.Channel).ToDictionary(g => g.Key, g => g.Count()),
            AverageResponseTimeHours = 0,
            AverageResolutionTimeHours = 0,
            SatisfactionRate = 0
        };

        // Calculate average response time
        var ticketsWithResponse = tickets.Where(t => t.FirstResponseAt.HasValue).ToList();
        if (ticketsWithResponse.Any())
        {
            var totalSeconds = ticketsWithResponse.Sum(t => (t.FirstResponseAt.Value - t.CreatedAt).TotalSeconds);
            statistics.AverageResponseTimeHours = totalSeconds / ticketsWithResponse.Count / 3600;
        }

        // Calculate average resolution time
        var resolvedTickets = tickets.Where(t => t.ResolvedAt.HasValue || t.ClosedAt.HasValue).ToList();
        if (resolvedTickets.Any())
        {
            var totalSeconds = resolvedTickets.Sum(t => t.GetResolutionTime().TotalSeconds);
            statistics.AverageResolutionTimeHours = totalSeconds / resolvedTickets.Count / 3600;
        }

        // Calculate satisfaction rate
        var ratedTickets = tickets.Where(t => t.SatisfactionRating.HasValue).ToList();
        if (ratedTickets.Any())
        {
            statistics.SatisfactionRate = ratedTickets.Average(t => t.SatisfactionRating.Value);
        }

        // Last 7 days tickets
        var last7Days = Enumerable.Range(0, 7)
            .Select(i => DateTime.UtcNow.Date.AddDays(-i))
            .Reverse()
            .ToList();

        statistics.Last7DaysTickets = last7Days
            .Select(date => new DailyTicketCount
            {
                Date = date,
                Count = tickets.Count(t => t.CreatedAt.Date == date)
            })
            .ToList();

        return statistics;
    }

    // Additional helper methods
    public async Task<int> GetUnassignedTicketCountAsync(Guid businessId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .CountAsync(t => t.BusinessId == businessId
                             && t.AssignedToAgentId == null
                             && t.Status != TicketStatus.Closed
                             && t.Status != TicketStatus.Resolved
                             && !t.IsDeleted, cancellationToken);
    }

    public async Task<IReadOnlyList<SupportTicket>> GetTicketsRequiringAttentionAsync(Guid businessId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.Customer)
            .Where(t => t.BusinessId == businessId
                        && !t.IsDeleted
                        && (t.Status == TicketStatus.Open
                            || t.Status == TicketStatus.WaitingForAgent
                            || (t.Status == TicketStatus.InProgress && t.IsOverdue())))
            .OrderByDescending(t => t.Priority)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}