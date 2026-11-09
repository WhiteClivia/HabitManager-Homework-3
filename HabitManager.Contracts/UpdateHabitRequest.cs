namespace HabitManager.Contracts;

public sealed record UpdateHabitRequest(string Name, string? Description, HabitScheduleDto Schedule);

