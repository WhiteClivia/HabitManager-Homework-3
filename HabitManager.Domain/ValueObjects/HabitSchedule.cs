using HabitManager.Domain.Enums;

namespace HabitManager.Domain.ValueObjects;

public sealed class HabitSchedule
{
    public RepeatType RepeatType { get; }
    public IReadOnlyCollection<DayOfWeek> DaysOfWeek { get; }
    public DateOnly? OnceDate { get; }

    private HabitSchedule(RepeatType repeatType, DayOfWeek[]? days = null, DateOnly? onceDate = null)
    {
        RepeatType = repeatType;
        DaysOfWeek = Array.AsReadOnly((days ?? []).Distinct().Order().ToArray());
        OnceDate = onceDate;
    }

    public static HabitSchedule Once(DateOnly date) => new(RepeatType.Once, onceDate: date);

    public static HabitSchedule Daily() => new(RepeatType.Daily);

    public static HabitSchedule Weekly(params DayOfWeek[] days)
    {
        if (days.Length == 0 || days.Any(day => !Enum.IsDefined(day)))
            throw new ArgumentException("Choose at least one valid weekday.", nameof(days));

        return new HabitSchedule(RepeatType.Weekly, days);
    }

    public bool IsScheduledFor(DateOnly date) => RepeatType switch
    {
        RepeatType.Once => date == OnceDate,
        RepeatType.Daily => true,
        RepeatType.Weekly => DaysOfWeek.Contains(date.DayOfWeek),
        _ => false
    };

    public override string ToString() => RepeatType switch
    {
        RepeatType.Once => $"Once: {OnceDate:yyyy-MM-dd}",
        RepeatType.Daily => "Daily",
        RepeatType.Weekly => $"Weekly: {string.Join(", ", DaysOfWeek)}",
        _ => "Unknown"
    };
}

