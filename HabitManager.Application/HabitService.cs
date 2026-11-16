using HabitManager.Contracts;
using HabitManager.Domain.Entities;
using HabitManager.Domain.ValueObjects;

namespace HabitManager.Application;

public sealed class HabitService(IHabitRepository repository)
{
    public async Task<HabitDto[]> ListAsync(string userId) =>
        (await repository.ListAsync(userId)).Select(ToDto).ToArray();

    public async Task<HabitDto> GetAsync(string userId, Guid id) =>
        ToDto(await FindAsync(userId, id));

    public async Task<HabitDto> CreateAsync(string userId, CreateHabitRequest request)
    {
        var habit = new Habit(userId, request.Name, request.Description ?? "", ToSchedule(request.Schedule));
        await repository.SaveAsync(habit);
        return ToDto(habit);
    }

    public Task<HabitDto> UpdateAsync(string userId, Guid id, UpdateHabitRequest request) =>
        ChangeAsync(userId, id, habit =>
            habit.Update(request.Name, request.Description ?? "", ToSchedule(request.Schedule)));

    public Task<HabitDto> ArchiveAsync(string userId, Guid id) =>
        ChangeAsync(userId, id, habit => habit.Archive());

    public Task<HabitDto> RestoreAsync(string userId, Guid id) =>
        ChangeAsync(userId, id, habit => habit.Restore());

    public Task<HabitDto> CompleteAsync(string userId, Guid id, DateOnly date) =>
        ChangeAsync(userId, id, habit => habit.MarkAsCompleted(date, allowExisting: true));

    public Task<HabitDto> UndoCompletionAsync(string userId, Guid id, DateOnly date) =>
        ChangeAsync(userId, id, habit => habit.RemoveCompletion(date));

    public async Task<IReadOnlyCollection<HabitCompletionDto>> GetCompletionsAsync(string userId, Guid id) =>
        (await FindAsync(userId, id)).Completions.Select(ToDto).OrderByDescending(item => item.Date).ToArray();

    private async Task<HabitDto> ChangeAsync(string userId, Guid id, Action<Habit> change)
    {
        var habit = await FindAsync(userId, id);
        change(habit);
        await repository.SaveAsync(habit);
        return ToDto(habit);
    }

    private async Task<Habit> FindAsync(string userId, Guid id) =>
        await repository.FindAsync(userId, id) ?? throw new HabitNotFoundException(id);

    private static HabitSchedule ToSchedule(HabitScheduleDto? schedule)
    {
        ArgumentNullException.ThrowIfNull(schedule);
        var days = schedule.DaysOfWeek?.ToArray() ?? [];
        return schedule.RepeatType?.Trim().ToUpperInvariant() switch
        {
            "ONCE" when schedule.OnceDate is DateOnly date && days.Length == 0 => HabitSchedule.Once(date),
            "DAILY" when schedule.OnceDate is null && days.Length == 0 => HabitSchedule.Daily(),
            "WEEKLY" when schedule.OnceDate is null => HabitSchedule.Weekly(days),
            _ => throw new ArgumentException("Use Daily, Weekly with weekdays, or Once with a date.")
        };
    }

    public static HabitDto ToDto(Habit habit) => new(
        habit.Id, habit.UserId, habit.Name, habit.Description, habit.Status.ToString(),
        new HabitScheduleDto(habit.Schedule.RepeatType.ToString(), habit.Schedule.DaysOfWeek, habit.Schedule.OnceDate),
        habit.CreatedAt, habit.UpdatedAt,
        habit.Completions.Select(ToDto).OrderByDescending(item => item.Date).ToArray());

    private static HabitCompletionDto ToDto(HabitCompletion completion) =>
        new(completion.Id, completion.HabitId, completion.Date);
}
