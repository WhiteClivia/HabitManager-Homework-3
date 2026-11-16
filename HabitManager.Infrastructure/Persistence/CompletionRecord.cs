namespace HabitManager.Infrastructure.Persistence;

public sealed class CompletionRecord
{
    public Guid Id { get; set; }
    public Guid HabitId { get; set; }
    public DateOnly Date { get; set; }
}
