
using BubbleShop.Domain.Entities;
using BubbleShop.Domain.Models;

namespace BubbleShop.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for Feedback entity operations
/// </summary>
public interface IFeedbackRepository : IRepository<Feedback>
{
    /// <summary>
    /// Get feedback by customer ID
    /// </summary>
    Task<IReadOnlyList<Feedback>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get feedback by business ID
    /// </summary>
    Task<IReadOnlyList<Feedback>> GetByBusinessIdAsync(Guid businessId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get feedback by rating
    /// </summary>
    Task<IReadOnlyList<Feedback>> GetByRatingAsync(int rating, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get feedback by date range
    /// </summary>
    Task<IReadOnlyList<Feedback>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get feedback by channel
    /// </summary>
    Task<IReadOnlyList<Feedback>> GetByChannelAsync(string channel, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get average rating for a business
    /// </summary>
    Task<double> GetAverageRatingAsync(Guid businessId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get rating distribution for a business
    /// </summary>
    Task<Dictionary<int, int>> GetRatingDistributionAsync(Guid businessId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get feedback summary statistics for a business
    /// </summary>
    Task<FeedbackStatistics> GetFeedbackStatisticsAsync(Guid businessId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search feedback by keyword
    /// </summary>
    Task<IReadOnlyList<Feedback>> SearchFeedbackAsync(string keyword, Guid? businessId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get recent feedback for a business
    /// </summary>
    Task<IReadOnlyList<Feedback>> GetRecentFeedbackAsync(Guid businessId, int count = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get positive feedback (rating >= 4)
    /// </summary>
    Task<IReadOnlyList<Feedback>> GetPositiveFeedbackAsync(Guid businessId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get negative feedback (rating <= 2)
    /// </summary>
    Task<IReadOnlyList<Feedback>> GetNegativeFeedbackAsync(Guid businessId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if customer has already given feedback
    /// </summary>
    Task<bool> HasCustomerGivenFeedbackAsync(Guid customerId, Guid? orderId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get feedback response time statistics
    /// </summary>
    Task<FeedbackResponseStats> GetResponseStatsAsync(Guid businessId, CancellationToken cancellationToken = default);
}
