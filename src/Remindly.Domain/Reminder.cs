namespace Remindly.Domain;

public sealed class Reminder
{
    private Reminder(string ownerId, string message, ReminderSchedule schedule, DateTime nowUtc)
    {
        if (string.IsNullOrWhiteSpace(ownerId)) throw new ArgumentException("Owner is required.", nameof(ownerId));
        if (string.IsNullOrWhiteSpace(message)) throw new ArgumentException("Message is required.", nameof(message));
        EnsureUtc(nowUtc, nameof(nowUtc));
        Id = Guid.NewGuid(); OwnerId = ownerId; Message = message.Trim(); Schedule = schedule ?? throw new ArgumentNullException(nameof(schedule));
        CreatedAtUtc = UpdatedAtUtc = nowUtc; Status = ReminderStatus.Active;
    }
    public Guid Id { get; }
    public string OwnerId { get; }
    public string Message { get; private set; }
    public ReminderSchedule Schedule { get; private set; }
    public ReminderStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; }
    public DateTime UpdatedAtUtc { get; private set; }
    public DateTime? PausedAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }
    public static Reminder Create(string ownerId, string message, ReminderSchedule schedule, DateTime nowUtc) => new(ownerId, message, schedule, nowUtc);
    public void Update(string message, ReminderSchedule schedule, DateTime nowUtc)
    {
        EnsureNotCompleted(); EnsureUtc(nowUtc, nameof(nowUtc));
        if (string.IsNullOrWhiteSpace(message)) throw new ArgumentException("Message is required.", nameof(message));
        Message = message.Trim(); Schedule = schedule ?? throw new ArgumentNullException(nameof(schedule)); UpdatedAtUtc = nowUtc;
    }
    public void Pause(DateTime nowUtc)
    {
        Require(ReminderStatus.Active); EnsureUtc(nowUtc, nameof(nowUtc));
        Status = ReminderStatus.Paused; PausedAtUtc = UpdatedAtUtc = nowUtc;
    }
    public void Resume(DateTime nowUtc)
    {
        Require(ReminderStatus.Paused); EnsureUtc(nowUtc, nameof(nowUtc));
        if (Schedule.ScheduledAtUtc <= nowUtc) throw new InvalidOperationException("The next scheduled time is in the past. Choose a new time before resuming.");
        Status = ReminderStatus.Active; PausedAtUtc = null; UpdatedAtUtc = nowUtc;
    }
    public void CompleteOccurrence(DateTime nowUtc)
    {
        Require(ReminderStatus.Active); EnsureUtc(nowUtc, nameof(nowUtc));
        if (Schedule.ScheduledAtUtc > nowUtc) throw new InvalidOperationException("The scheduled occurrence is not due yet.");
        if (Schedule.IsRecurring) Schedule.Advance();
        else { Status = ReminderStatus.Completed; CompletedAtUtc = nowUtc; }
        UpdatedAtUtc = nowUtc;
    }
    public void Finish(DateTime nowUtc)
    {
        EnsureNotCompleted(); EnsureUtc(nowUtc, nameof(nowUtc));
        Status = ReminderStatus.Completed; CompletedAtUtc = UpdatedAtUtc = nowUtc; PausedAtUtc = null;
    }
    private void EnsureNotCompleted() { if (Status == ReminderStatus.Completed) throw new InvalidOperationException("Completed reminders cannot be changed."); }
    private void Require(ReminderStatus status) { if (Status != status) throw new InvalidOperationException($"This operation requires status {status}."); }
    private static void EnsureUtc(DateTime value, string name) { if (value.Kind != DateTimeKind.Utc) throw new ArgumentException("Timestamp must be UTC.", name); }
}
