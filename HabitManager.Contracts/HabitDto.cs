namespace HabitManager.Contracts;

public sealed record HabitDto(
    Guid Id,
    string UserId,
    string Name,
    string Description,
    string Status,
    HabitScheduleDto Schedule,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyCollection<HabitCompletionDto> Completions);

