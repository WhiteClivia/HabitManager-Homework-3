namespace HabitManager.Contracts;

public sealed record HabitCompletionDto(
    Guid Id,
    Guid HabitId,
    DateOnly Date);

