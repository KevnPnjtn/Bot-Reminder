using Remindly.Domain;

namespace Remindly.UnitTests;

public class RecurrenceTests
{
    [Fact]
    public void Weekly_interval_uses_selected_weekdays_and_every_n_weeks()
    {
        var recurrence = new Recurrence.Weekly(2, new TimeOnly(9, 0), new[] { DayOfWeek.Monday, DayOfWeek.Wednesday });
        Assert.Equal(new DateTime(2026, 1, 7, 9, 0, 0), recurrence.NextLocal(new DateTime(2026, 1, 5, 9, 0, 0)));
        Assert.Equal(new DateTime(2026, 1, 19, 9, 0, 0), recurrence.NextLocal(new DateTime(2026, 1, 7, 9, 0, 0)));
    }
}
