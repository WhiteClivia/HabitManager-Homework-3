namespace HabitManager.Contracts;

public sealed record CreateHabitRequest(string Name, string? Description, HabitScheduleDto Schedule);


