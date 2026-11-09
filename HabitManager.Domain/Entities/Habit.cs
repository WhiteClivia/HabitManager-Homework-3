using HabitManager.Domain.Enums;
using HabitManager.Domain.ValueObjects;

namespace HabitManager.Domain.Entities;

public sealed class Habit
{
    private const int MaxNameLength = 100;
    private const int MaxDescriptionLength = 1000;

    private readonly List<HabitCompletion> _completions = [];

    public Guid Id { get; }

    public string UserId { get; }

    public string Name { get; private set; }

    public string Description { get; private set; }

    public HabitStatus Status { get; private set; }

    public DateTime CreatedAt { get; }

    public DateTime UpdatedAt { get; private set; }

    public HabitSchedule Schedule { get; private set; }

    public IReadOnlyCollection<HabitCompletion> Completions =>
        _completions.AsReadOnly();

    public Habit(
        string userId,
        string name,
        string description,
        HabitSchedule schedule)
        : this(Guid.NewGuid(), userId, name, description, schedule, HabitStatus.Active, DateTime.UtcNow)
    {
    }

    private Habit(
        Guid id, string userId, string name, string description, HabitSchedule schedule,
        HabitStatus status, DateTime createdAt)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("The habit ID cannot be empty.", nameof(id));

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException(
                "The habit must belong to a user.",
                nameof(userId));
        }

        ValidateName(name);
        ValidateDescription(description);

        ArgumentNullException.ThrowIfNull(schedule);

        Id = id;
        UserId = userId;
        Name = name.Trim();
        Description = description.Trim();
        Schedule = schedule;

        Status = status;

        CreatedAt = createdAt;
        UpdatedAt = CreatedAt;
    }

    public static Habit Rehydrate(
        Guid id, string userId, string name, string description, HabitSchedule schedule,
        HabitStatus status, DateTime createdAt, DateTime updatedAt,
        IEnumerable<HabitCompletion> completions)
    {
        if (!Enum.IsDefined(status))
            throw new ArgumentOutOfRangeException(nameof(status));

        var habit = new Habit(id, userId, name, description, schedule, status, createdAt);
        foreach (var completion in completions)
        {
            if (completion.HabitId != id || habit.IsCompletedOn(completion.Date))
                throw new ArgumentException("Invalid completion history.", nameof(completions));

            habit._completions.Add(completion);
        }

        habit.UpdatedAt = updatedAt;
        return habit;
    }

    public void Update(
        string name,
        string description,
        HabitSchedule schedule)
    {
        if (Status == HabitStatus.Archived)
        {
            throw new HabitRuleException(
                "Archived habits cannot be edited.");
        }

        ValidateName(name);
        ValidateDescription(description);

        ArgumentNullException.ThrowIfNull(schedule);

        Name = name.Trim();
        Description = description.Trim();
        Schedule = schedule;

        UpdatedAt = DateTime.UtcNow;
    }

    public void Archive()
    {
        if (Status == HabitStatus.Archived)
        {
            return;
        }

        Status = HabitStatus.Archived;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Restore()
    {
        if (Status == HabitStatus.Active)
        {
            return;
        }

        Status = HabitStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public HabitCompletion MarkAsCompleted(DateOnly date, bool allowExisting = false)
    {
        if (Status == HabitStatus.Archived)
        {
            throw new HabitRuleException(
                "Archived habits cannot be marked as completed.");
        }

        if (allowExisting && _completions.FirstOrDefault(item => item.Date == date) is { } existing)
            return existing;

        if (!Schedule.IsScheduledFor(date))
        {
            throw new HabitRuleException(
                $"The habit is not scheduled for {date}.");
        }

        if (IsCompletedOn(date))
        {
            throw new HabitRuleException(
                $"The habit is already marked as completed for {date}.");
        }

        var completion = new HabitCompletion(Id, date);

        _completions.Add(completion);
        UpdatedAt = DateTime.UtcNow;

        return completion;
    }

    public void RemoveCompletion(DateOnly date)
    {
        if (Status == HabitStatus.Archived)
            throw new HabitRuleException("Archived habits cannot be changed.");

        if (_completions.RemoveAll(item => item.Date == date) > 0)
            UpdatedAt = DateTime.UtcNow;
    }

    public bool IsCompletedOn(DateOnly date)
    {
        return _completions.Any(completion => completion.Date == date);
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "The habit name cannot be empty.",
                nameof(name));
        }

        if (name.Trim().Length > MaxNameLength)
        {
            throw new ArgumentException(
                $"The habit name cannot be longer than {MaxNameLength} characters.",
                nameof(name));
        }
    }

    private static void ValidateDescription(string description)
    {
        if (description is null)
        {
            throw new ArgumentNullException(nameof(description));
        }

        if (description.Length > MaxDescriptionLength)
        {
            throw new ArgumentException(
                $"The habit description cannot be longer than {MaxDescriptionLength} characters.",
                nameof(description));
        }
    }
}


