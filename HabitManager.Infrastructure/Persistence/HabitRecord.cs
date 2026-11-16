using HabitManager.Domain.Enums;

namespace HabitManager.Infrastructure.Persistence;

public sealed class HabitRecord
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public HabitStatus Status { get; set; }
    public RepeatType RepeatType { get; set; }
    public int WeekdaysMask { get; set; }
    public DateOnly? OnceDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid Version { get; set; }
    public List<CompletionRecord> Completions { get; set; } = [];
}
