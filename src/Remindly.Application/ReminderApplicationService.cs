namespace Remindly.Application;

using Remindly.Domain;

public sealed class ReminderApplicationService(IReminderRepository repository, IClock clock)
{
    public async Task<Reminder> CreateAsync(string ownerId, string message, DateTime localScheduledAt, string timeZoneId, Recurrence? recurrence = null, CancellationToken cancellationToken = default)
    {
        var zone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        var scheduledUtc = ReminderSchedule.LocalToUtc(localScheduledAt, zone);
        var now = clock.UtcNow;
        if (scheduledUtc <= now) throw new ArgumentException("Reminder time must be in the future.", nameof(localScheduledAt));
        var schedule = recurrence is null ? ReminderSchedule.OneTime(scheduledUtc, timeZoneId) : ReminderSchedule.Recurring(scheduledUtc, timeZoneId, recurrence);
        var reminder = Reminder.Create(ownerId, message, schedule, now);
        await repository.AddAsync(reminder, cancellationToken);
        return reminder;
    }

    public async Task<Reminder> UpdateAsync(Guid id, string ownerId, string message, DateTime localScheduledAt, string timeZoneId, Recurrence? recurrence = null, CancellationToken cancellationToken = default)
    {
        var reminder = await GetOwnedAsync(id, ownerId, cancellationToken);
        var zone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        var scheduledUtc = ReminderSchedule.LocalToUtc(localScheduledAt, zone);
        var now = clock.UtcNow;
        if (scheduledUtc <= now) throw new ArgumentException("Reminder time must be in the future.", nameof(localScheduledAt));
        var schedule = recurrence is null ? ReminderSchedule.OneTime(scheduledUtc, timeZoneId) : ReminderSchedule.Recurring(scheduledUtc, timeZoneId, recurrence);
        reminder.Update(message, schedule, now);
        await repository.SaveAsync(reminder, cancellationToken);
        return reminder;
    }

    public Task PauseAsync(Guid id, string ownerId, CancellationToken token = default) => ChangeAsync(id, ownerId, x => x.Pause(clock.UtcNow), token);
    public Task ResumeAsync(Guid id, string ownerId, CancellationToken token = default) => ChangeAsync(id, ownerId, x => x.Resume(clock.UtcNow), token);
    public Task CompleteOccurrenceAsync(Guid id, string ownerId, CancellationToken token = default) => ChangeAsync(id, ownerId, x => x.CompleteOccurrence(clock.UtcNow), token);
    public Task FinishAsync(Guid id, string ownerId, CancellationToken token = default) => ChangeAsync(id, ownerId, x => x.Finish(clock.UtcNow), token);
    public Task<IReadOnlyList<Reminder>> ListAsync(string ownerId, ReminderStatus? status = null, CancellationToken token = default) => repository.ListAsync(ownerId, status, token);

    private async Task ChangeAsync(Guid id, string ownerId, Action<Reminder> change, CancellationToken token)
    {
        var reminder = await GetOwnedAsync(id, ownerId, token);
        change(reminder);
        await repository.SaveAsync(reminder, token);
    }
    private async Task<Reminder> GetOwnedAsync(Guid id, string ownerId, CancellationToken token) =>
        await repository.GetAsync(id, ownerId, token) ?? throw new KeyNotFoundException("Reminder not found for this owner.");
}



