// Infrastructure/Persistence/Repositories/FeedbackRepository.cs
using Microsoft.EntityFrameworkCore;
using BubbleShop.Domain.Entities;
using BubbleShop.Domain.Interfaces.Repositories;
using BubbleShop.Infrastructure.Persistence;
using BubbleShop.Domain.Models;

namespace BubbleShop.Infrastructure.Persistence.Repositories;

public class FeedbackRepository : Repository<Feedback>, IFeedbackRepository
{
    public FeedbackRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Feedback>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(f => f.Customer)
            .Include(f => f.Order)
            .Where(f => f.CustomerId == customerId && !f.IsDeleted)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Feedback>> GetByBusinessIdAsync(Guid businessId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(f => f.Customer)
            .Include(f => f.Order)
            .Where(f => f.BusinessId == businessId && !f.IsDeleted)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Feedback>> GetByRatingAsync(int rating, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(f => f.Customer)
            .Where(f => f.Rating == rating && !f.IsDeleted)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Feedback>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(f => f.Customer)
            .Where(f => f.CreatedAt >= startDate && f.CreatedAt <= endDate && !f.IsDeleted)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Feedback>> GetByChannelAsync(string channel, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(f => f.Channel == channel && !f.IsDeleted)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<double> GetAverageRatingAsync(Guid businessId, CancellationToken cancellationToken = default)
    {
        var feedbacks = await _dbSet
            .Where(f => f.BusinessId == businessId && !f.IsDeleted)
            .ToListAsync(cancellationToken);

        if (!feedbacks.Any())
            return 0;

        return feedbacks.Average(f => f.Rating);
    }

    public async Task<Dictionary<int, int>> GetRatingDistributionAsync(Guid businessId, CancellationToken cancellationToken = default)
    {
        var feedbacks = await _dbSet
            .Where(f => f.BusinessId == businessId && !f.IsDeleted)
            .ToListAsync(cancellationToken);

        return feedbacks
            .GroupBy(f => f.Rating)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<FeedbackStatistics> GetFeedbackStatisticsAsync(Guid businessId, CancellationToken cancellationToken = default)
    {
        var feedbacks = await _dbSet
            .Include(f => f.Customer)
            .Where(f => f.BusinessId == businessId && !f.IsDeleted)
            .ToListAsync(cancellationToken);

        var stats = new FeedbackStatistics
        {
            TotalFeedback = feedbacks.Count,
            AverageRating = feedbacks.Any() ? feedbacks.Average(f => f.Rating) : 0,
            PositiveCount = feedbacks.Count(f => f.Rating >= 4),
            NeutralCount = feedbacks.Count(f => f.Rating == 3),
            NegativeCount = feedbacks.Count(f => f.Rating <= 2),
            RatingDistribution = feedbacks.GroupBy(f => f.Rating).ToDictionary(g => g.Key, g => g.Count()),
            FeedbackByChannel = feedbacks.GroupBy(f => f.Channel).ToDictionary(g => g.Key, g => g.Count()),
            FeedbackByCategory = feedbacks.GroupBy(f => f.Category).ToDictionary(g => g.Key, g => g.Count()),
            RecentFeedback = feedbacks
                .OrderByDescending(f => f.CreatedAt)
                .Take(10)
                .Select(f => new RecentFeedbackDto
                {
                    Id = f.Id,
                    CustomerName = f.Customer?.Name ?? "Anonymous",
                    Rating = f.Rating,
                    Comment = f.Comment,
                    Channel = f.Channel,
                    CreatedAt = f.CreatedAt
                })
                .ToList()
        };

        // Calculate trend
        var now = DateTime.UtcNow;
        var currentMonthStart = new DateTime(now.Year, now.Month, 1);
        var previousMonthStart = currentMonthStart.AddMonths(-1);

        var currentMonthFeedbacks = feedbacks.Where(f => f.CreatedAt >= currentMonthStart).ToList();
        var previousMonthFeedbacks = feedbacks.Where(f => f.CreatedAt >= previousMonthStart && f.CreatedAt < currentMonthStart).ToList();

        stats.Trend = new FeedbackTrend
        {
            CurrentMonthAverage = currentMonthFeedbacks.Any() ? currentMonthFeedbacks.Average(f => f.Rating) : 0,
            PreviousMonthAverage = previousMonthFeedbacks.Any() ? previousMonthFeedbacks.Average(f => f.Rating) : 0,
            ChangePercentage = 0
        };

        if (stats.Trend.PreviousMonthAverage > 0)
        {
            stats.Trend.ChangePercentage = ((stats.Trend.CurrentMonthAverage - stats.Trend.PreviousMonthAverage) / stats.Trend.PreviousMonthAverage) * 100;
        }

        // Last 7 days
        stats.Trend.Last7Days = Enumerable.Range(0, 7)
            .Select(i => DateTime.UtcNow.Date.AddDays(-i))
            .Reverse()
            .Select(date =>
            {
                var dayFeedbacks = feedbacks.Where(f => f.CreatedAt.Date == date).ToList();
                return new DailyFeedbackCount
                {
                    Date = date,
                    Count = dayFeedbacks.Count,
                    AverageRating = dayFeedbacks.Any() ? dayFeedbacks.Average(f => f.Rating) : 0
                };
            })
            .ToList();

        return stats;
    }

    public async Task<IReadOnlyList<Feedback>> SearchFeedbackAsync(string keyword, Guid? businessId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(f => f.Customer)
            .Where(f => !f.IsDeleted);

        if (businessId.HasValue)
        {
            query = query.Where(f => f.BusinessId == businessId.Value);
        }

        var lowerKeyword = keyword.ToLower();

        return await query
            .Where(f => f.Comment != null && f.Comment.ToLower().Contains(lowerKeyword) ||
                        f.Customer != null &&
f.Customer.Name.ToLower().Contains(lowerKeyword) ||
                        f.Tags.Any(t => t.ToLower().Contains(lowerKeyword)))
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Feedback>> GetRecentFeedbackAsync(Guid businessId, int count = 10, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(f => f.Customer)
            .Where(f => f.BusinessId == businessId && !f.IsDeleted)
            .OrderByDescending(f => f.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Feedback>> GetPositiveFeedbackAsync(Guid businessId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(f => f.Customer)
            .Where(f => f.BusinessId == businessId && f.Rating >= 4 && !f.IsDeleted)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Feedback>> GetNegativeFeedbackAsync(Guid businessId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(f => f.Customer)
            .Where(f => f.BusinessId == businessId && f.Rating <= 2 && !f.IsDeleted)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasCustomerGivenFeedbackAsync(Guid customerId, Guid? orderId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(f => f.CustomerId == customerId && !f.IsDeleted);

        if (orderId.HasValue)
        {
            query = query.Where(f => f.OrderId == orderId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<FeedbackResponseStats> GetResponseStatsAsync(Guid businessId, CancellationToken cancellationToken = default)
    {
        var feedbacks = await _dbSet
            .Where(f => f.BusinessId == businessId && !f.IsDeleted)
            .ToListAsync(cancellationToken);

        var responded = feedbacks.Where(f => f.RespondedAt.HasValue).ToList();
        var unresponded = feedbacks.Where(f => !f.RespondedAt.HasValue).ToList();

        var stats = new FeedbackResponseStats
        {
            TotalResponded = responded.Count,
            TotalUnresponded = unresponded.Count
        };

        if (responded.Any())
        {
            var responseTimes = responded
                .Where(f => f.RespondedAt.HasValue)
                .Select(f => (f.RespondedAt.Value - f.CreatedAt).TotalHours)
                .ToList();

            stats.AverageResponseTimeHours = responseTimes.Average();
            stats.MaxResponseTimeHours = responseTimes.Max();
            stats.MinResponseTimeHours = responseTimes.Min();
        }

        return stats;
    }
}