namespace Remindly.Domain;

public sealed class ReminderSchedule
{
    private readonly TimeZoneInfo _zone;
    private ReminderSchedule(DateTime utc, string zoneId, Recurrence? recurrence)
    {
        if (utc.Kind != DateTimeKind.Utc) throw new ArgumentException("Scheduled timestamp must be UTC.", nameof(utc));
        if (string.IsNullOrWhiteSpace(zoneId)) throw new ArgumentException("Time zone is required.", nameof(zoneId));
        _zone = TimeZoneInfo.FindSystemTimeZoneById(zoneId);
        TimeZoneId = zoneId; ScheduledAtUtc = utc; Recurrence = recurrence;
        if (recurrence != null) { var local = TimeZoneInfo.ConvertTimeFromUtc(utc, _zone); ValidateLocal(utc); if (local.TimeOfDay != recurrence.LocalTime.ToTimeSpan()) throw new ArgumentException("Recurring local time must match the scheduled time.", nameof(recurrence)); }
    }
    public DateTime ScheduledAtUtc { get; private set; }
    public string TimeZoneId { get; }
    public Recurrence? Recurrence { get; }
    public bool IsRecurring => Recurrence != null;
    public static ReminderSchedule OneTime(DateTime utc, string zoneId) => new(utc, zoneId, null);
    public static ReminderSchedule Recurring(DateTime utc, string zoneId, Recurrence recurrence) => new(utc, zoneId, recurrence ?? throw new ArgumentNullException(nameof(recurrence)));

    public void Advance()
    {
        if (Recurrence is null) throw new InvalidOperationException("One-time reminders have no next occurrence.");
        var local = DateTime.SpecifyKind(TimeZoneInfo.ConvertTimeFromUtc(ScheduledAtUtc, _zone), DateTimeKind.Unspecified);
        var next = Recurrence.NextLocal(local);
        ScheduledAtUtc = LocalToUtc(next, _zone);
    }
    public static DateTime LocalToUtc(DateTime local, TimeZoneInfo zone)
    {
        local = DateTime.SpecifyKind(local, DateTimeKind.Unspecified);
        if (zone.IsInvalidTime(local)) throw new ArgumentException("This local time does not exist due to a daylight-saving change.", nameof(local));
        if (zone.IsAmbiguousTime(local)) throw new ArgumentException("This local time is ambiguous due to a daylight-saving change.", nameof(local));
        return TimeZoneInfo.ConvertTimeToUtc(local, zone);
    }
    private void ValidateLocal(DateTime utc) => _ = LocalToUtc(TimeZoneInfo.ConvertTimeFromUtc(utc, _zone), _zone);
}

