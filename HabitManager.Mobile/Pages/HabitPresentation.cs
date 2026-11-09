using HabitManager.Contracts;
using HabitManager.Domain.ValueObjects;

namespace HabitManager.Mobile.Pages;

internal static class HabitPresentation
{
    public static bool IsArchived(this HabitDto habit) => habit.Status == "Archived";

    public static bool IsCompletedOn(this HabitDto habit, DateOnly date) =>
        habit.Completions.Any(completion => completion.Date == date);

    public static HabitSchedule ToDomain(this HabitScheduleDto schedule) => schedule.RepeatType switch
    {
        "Once" when schedule.OnceDate is DateOnly date => HabitSchedule.Once(date),
        "Daily" => HabitSchedule.Daily(),
        "Weekly" => HabitSchedule.Weekly(schedule.DaysOfWeek?.ToArray() ?? []),
        _ => throw new Services.HabitClientException("The API returned an unknown schedule.")
    };

    public static Button Button(string text, Func<Task> action)
    {
        var button = new Button { Text = text };
        button.Clicked += async (_, _) => await action();
        return button;
    }
}
