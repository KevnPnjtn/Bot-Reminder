namespace Remindly.Domain;

public abstract record Recurrence
{
    protected Recurrence(int interval, TimeOnly localTime)
    {
        if (interval < 1) throw new ArgumentOutOfRangeException(nameof(interval), "Interval must be positive.");
        Interval = interval; LocalTime = localTime;
    }
    public int Interval { get; }
    public TimeOnly LocalTime { get; }
    public abstract DateTime NextLocal(DateTime after);

    public sealed record Daily : Recurrence
    {
        public Daily(int interval, TimeOnly localTime) : base(interval, localTime) { }
        public override DateTime NextLocal(DateTime after)
        {
            var next = DateTime.SpecifyKind(after.Date + LocalTime.ToTimeSpan(), DateTimeKind.Unspecified);
            while (next <= after) next = next.AddDays(Interval);
            return next;
        }
    }

    public sealed record Weekly : Recurrence
    {
        public Weekly(int interval, TimeOnly localTime, IEnumerable<DayOfWeek> days) : base(interval, localTime)
        {
            Days = Array.AsReadOnly(days?.Distinct().Order().ToArray() ?? throw new ArgumentNullException(nameof(days)));
            if (Days.Count == 0) throw new ArgumentException("At least one weekday is required.", nameof(days));
        }
        public IReadOnlyList<DayOfWeek> Days { get; }
        public override DateTime NextLocal(DateTime after)
        {
            var start = DateTime.SpecifyKind(after.Date + LocalTime.ToTimeSpan(), DateTimeKind.Unspecified);
            var baseWeek = WeekIndex(start);
            for (var i = 0; i <= Interval * 7 + 7; i++)
            {
                var candidate = start.AddDays(i);
                if (candidate > after && Days.Contains(candidate.DayOfWeek) && (WeekIndex(candidate) - baseWeek) % Interval == 0) return candidate;
            }
            throw new InvalidOperationException("Could not calculate next weekly occurrence.");
        }
        private static int WeekIndex(DateTime value) { var monday = value.Date.AddDays(-(((int)value.DayOfWeek + 6) % 7)); return (int)((monday - DateTime.UnixEpoch.Date).TotalDays / 7); }
    }

    public sealed record Monthly : Recurrence
    {
        public Monthly(int interval, TimeOnly localTime, int day) : base(interval, localTime)
        {
            if (day is < 1 or > 31) throw new ArgumentOutOfRangeException(nameof(day));
            Day = day;
        }
        public int Day { get; }
        public override DateTime NextLocal(DateTime after)
        {
            var firstMonth = new DateTime(after.Year, after.Month, 1);
            for (var offset = 0; offset <= 1200; offset += Interval)
            {
                var month = firstMonth.AddMonths(offset);
                var day = Math.Min(Day, DateTime.DaysInMonth(month.Year, month.Month));
                var candidate = DateTime.SpecifyKind(month.AddDays(day - 1) + LocalTime.ToTimeSpan(), DateTimeKind.Unspecified);
                if (candidate > after) return candidate;
            }
            throw new InvalidOperationException("Could not calculate next monthly occurrence.");
        }
    }
}


