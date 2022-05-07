namespace KanbanBoard.Utils;

public static class DateTimeExtensions
{
    public static DateTime? SetKindUtc(this DateTime? dateTime)
    {
        if (dateTime.HasValue)
            return dateTime.Value.SetKindUtc();
        
        return null;
    }
    public static DateTime SetKindUtc(this DateTime dateTime)
    {
        if (dateTime.Kind == DateTimeKind.Utc) { return dateTime; }
        return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
    }

    public static DateTime RoundToSeconds(this DateTime dateTime) => new DateTime(dateTime.Ticks / 1000_0000 * 1000_0000);
}