namespace HabitManager.Application;

public sealed class HabitNotFoundException(Guid id) : Exception($"Habit {id} was not found.");
