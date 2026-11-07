namespace Remindly.Application;

using Remindly.Domain;

public interface IClock
{
    DateTime UtcNow { get; }
}

public interface IReminderRepository
{
    Task AddAsync(Reminder reminder, CancellationToken cancellationToken = default);
    Task<Reminder?> GetAsync(Guid reminderId, string ownerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Reminder>> ListAsync(string ownerId, ReminderStatus? status = null, CancellationToken cancellationToken = default);
    Task SaveAsync(Reminder reminder, CancellationToken cancellationToken = default);
}
