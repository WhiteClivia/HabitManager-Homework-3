using HabitManager.Domain.Entities;

namespace HabitManager.Application;

public interface IHabitRepository
{
    Task<IReadOnlyList<Habit>> ListAsync(string userId);
    Task<Habit?> FindAsync(string userId, Guid id);
    Task SaveAsync(Habit habit);
}
