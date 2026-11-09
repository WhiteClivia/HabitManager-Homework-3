namespace HabitManager.Contracts;

public sealed record HabitScheduleDto(
    string RepeatType,
    IReadOnlyCollection<DayOfWeek>? DaysOfWeek = null,
    DateOnly? OnceDate = null);

