using Remindly.Domain;

namespace Remindly.UnitTests;

public class ReminderTests
{
    private static readonly DateTime Now = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static Reminder OneTime(DateTime when) => Reminder.Create("owner-1", "  call mom  ", ReminderSchedule.OneTime(when, "UTC"), Now);

    [Fact]
    public void Create_trims_message_and_starts_active() 
    {
        var reminder = OneTime(Now.AddHours(1));
        Assert.Equal("call mom", reminder.Message);
        Assert.Equal(ReminderStatus.Active, reminder.Status);
    }

    [Fact]
    public void Pause_and_resume_enforce_state_and_future_schedule()
    {
        var reminder = OneTime(Now.AddHours(1));
        reminder.Pause(Now);
        Assert.Equal(ReminderStatus.Paused, reminder.Status);
        Assert.Throws<InvalidOperationException>(() => reminder.Resume(Now.AddHours(2)));
        Assert.Equal(ReminderStatus.Paused, reminder.Status);
    }

    [Fact]
    public void Complete_one_time_reminder_marks_completed()
    {
        var reminder = OneTime(Now.AddHours(1));
        reminder.CompleteOccurrence(Now.AddHours(1));
        Assert.Equal(ReminderStatus.Completed, reminder.Status);
        Assert.NotNull(reminder.CompletedAtUtc);
    }

    [Fact]
    public void Complete_recurring_occurrence_advances_and_remains_active()
    {
        var start = new DateTime(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc);
        var recurrence = new Recurrence.Daily(1, new TimeOnly(9, 0));
        var reminder = Reminder.Create("owner", "test", ReminderSchedule.Recurring(start, "UTC", recurrence), start.AddDays(-1));
        reminder.CompleteOccurrence(start);
        Assert.Equal(ReminderStatus.Active, reminder.Status);
        Assert.Equal(start.AddDays(1), reminder.Schedule.ScheduledAtUtc);
    }

    [Fact]
    public void Monthly_day_31_uses_month_end()
    {
        var recurrence = new Recurrence.Monthly(1, new TimeOnly(9, 0), 31);
        Assert.Equal(new DateTime(2026, 2, 28, 9, 0, 0), recurrence.NextLocal(new DateTime(2026, 1, 31, 9, 0, 0)));
    }

    [Fact]
    public void Invalid_and_ambiguous_local_times_are_rejected()
    {
        var eastern = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
        Assert.Throws<ArgumentException>(() => ReminderSchedule.LocalToUtc(new DateTime(2026, 3, 8, 2, 30, 0), eastern));
        Assert.Throws<ArgumentException>(() => ReminderSchedule.LocalToUtc(new DateTime(2026, 11, 1, 1, 30, 0), eastern));
    }

    [Fact]
    public void Completed_reminder_cannot_be_updated()
    {
        var reminder = OneTime(Now.AddHours(1));
        reminder.Finish(Now);
        Assert.Throws<InvalidOperationException>(() => reminder.Update("new", reminder.Schedule, Now.AddMinutes(1)));
    }
}
