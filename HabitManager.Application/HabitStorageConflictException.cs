namespace HabitManager.Application;

public sealed class HabitStorageConflictException(string message, Exception inner)
    : Exception(message, inner);
