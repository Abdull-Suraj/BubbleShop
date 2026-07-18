
using BubbleShop.Application.DTOs;
using BubbleShop.Domain.Entities;
using BubbleShop.Domain.Enums;
using BubbleShop.Domain.Interfaces.Repositories;


namespace BubbleShop.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for Support Ticket entity operations
/// </summary>
public interface ISupportTicketRepository : IRepository<SupportTicket>
{
    /// <summary>
    /// Get ticket by ticket number
    /// </summary>
    Task<SupportTicket?> GetByTicketNumberAsync(string ticketNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all tickets for a customer
    /// </summary>
    Task<IReadOnlyList<SupportTicket>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all tickets for a business
    /// </summary>
    Task<IReadOnlyList<SupportTicket>> GetByBusinessIdAsync(Guid businessId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get tickets by status
    /// </summary>
    Task<IReadOnlyList<SupportTicket>> GetByStatusAsync(TicketStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get tickets by priority
    /// </summary>
    Task<IReadOnlyList<SupportTicket>> GetByPriorityAsync(TicketPriority priority, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get tickets by category
    /// </summary>
    Task<IReadOnlyList<SupportTicket>> GetByCategoryAsync(TicketCategory category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get tickets assigned to a specific agent
    /// </summary>
    Task<IReadOnlyList<SupportTicket>> GetByAssignedAgentAsync(Guid agentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get open tickets for a business
    /// </summary>
    Task<IReadOnlyList<SupportTicket>> GetOpenTicketsByBusinessIdAsync(Guid businessId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get overdue tickets
    /// </summary>
    Task<IReadOnlyList<SupportTicket>> GetOverdueTicketsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get tickets created within date range
    /// </summary>
    Task<IReadOnlyList<SupportTicket>> GetTicketsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get tickets by channel
    /// </summary>
    Task<IReadOnlyList<SupportTicket>> GetTicketsByChannelAsync(string channel, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get count of tickets by status for a business
    /// </summary>
    Task<Dictionary<TicketStatus, int>> GetTicketCountsByStatusAsync(Guid businessId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get average response time for tickets
    /// </summary>
    Task<TimeSpan> GetAverageResponseTimeAsync(Guid businessId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get average resolution time for tickets
    /// </summary>
    Task<TimeSpan> GetAverageResolutionTimeAsync(Guid businessId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search tickets by keyword
    /// </summary>
    Task<IReadOnlyList<SupportTicket>> SearchTicketsAsync(string keyword, Guid? businessId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get ticket statistics
    /// </summary>
    Task<TicketStatistics> GetTicketStatisticsAsync(Guid businessId, CancellationToken cancellationToken = default);
}
