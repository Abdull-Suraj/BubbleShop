namespace BubbleShop.Domain.Entities;

public class WorkingHours
{
    public bool Is24Hours { get; set; } = true;
    public TimeOnly OpenTime { get; set; } = new TimeOnly(9, 0);
    public TimeOnly CloseTime { get; set; } = new TimeOnly(21, 0);
    public List<DayOfWeek> WorkingDays { get; set; } = new()
    {
        DayOfWeek.Monday,
        DayOfWeek.Tuesday,
        DayOfWeek.Wednesday,
        DayOfWeek.Thursday,
        DayOfWeek.Friday,
        DayOfWeek.Saturday
    };

    public bool IsOpen(DateTime time)
    {
        if (Is24Hours) return true;
        if (!WorkingDays.Contains(time.DayOfWeek)) return false;

        var timeOnly = TimeOnly.FromDateTime(time);
        return timeOnly >= OpenTime && timeOnly <= CloseTime;
    }
}
