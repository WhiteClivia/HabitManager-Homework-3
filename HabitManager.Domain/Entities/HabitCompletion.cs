namespace HabitManager.Domain.Entities;

public sealed class HabitCompletion
{
    public Guid Id { get; }
    public Guid HabitId { get; }
    public DateOnly Date { get; }

    public HabitCompletion(Guid habitId, DateOnly date) : this(Guid.NewGuid(), habitId, date)
    {
    }

    public HabitCompletion(Guid id, Guid habitId, DateOnly date)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("The completion ID cannot be empty.", nameof(id));

        if (habitId == Guid.Empty)
            throw new ArgumentException("The habit ID cannot be empty.", nameof(habitId));

        Id = id;
        HabitId = habitId;
        Date = date;
    }
}

