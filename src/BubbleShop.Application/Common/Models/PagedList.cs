namespace BubbleShop.Application.Common.Models;

public sealed record PagedList<T>(IReadOnlyCollection<T> Items, int PageNumber, int PageSize, int TotalCount);
