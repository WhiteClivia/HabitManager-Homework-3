using HabitManager.Application;
using HabitManager.Domain.Entities;
using HabitManager.Domain.TestData;

namespace HabitManager.Infrastructure;

public sealed class InMemoryHabitRepository : IHabitRepository

{
    private readonly Dictionary<Guid, Habit> _habits =
        TestDataGenerator.GenerateHabits().ToDictionary(habit => habit.Id);

    public Task<IReadOnlyList<Habit>> ListAsync(string userId) =>
        Task.FromResult<IReadOnlyList<Habit>>(_habits.Values.Where(habit => habit.UserId == userId)
            .OrderBy(habit => habit.CreatedAt).ToArray());

    public Task<Habit?> FindAsync(string userId, Guid id) =>
        Task.FromResult(_habits.TryGetValue(id, out var habit) && habit.UserId == userId ? habit : null);

    public Task SaveAsync(Habit habit)
    {
        _habits[habit.Id] = habit;
        return Task.CompletedTask;
    }
}
