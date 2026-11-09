using HabitManager.Contracts;
using HabitManager.Mobile.Services;

namespace HabitManager.Mobile.Pages;

internal sealed class HabitEditorPage : ApiPage
{
    private readonly Entry _name = new() { Placeholder = "Name" };
    private readonly Editor _description = new() { Placeholder = "Description (optional)", AutoSize = EditorAutoSizeOption.TextChanges };
    private readonly Picker _repeat = new() { Title = "Repeat", ItemsSource = new[] { "Daily", "Weekly", "Once" } };
    private readonly DatePicker _onceDate = new() { Date = DateTime.Today };
    private readonly VerticalStackLayout _weekdays = new();
    private readonly Dictionary<DayOfWeek, CheckBox> _checks = new();
    private readonly Label _onceLabel = new() { Text = "Scheduled date" };

    public HabitEditorPage(IHabitClient client, HabitDto? habit = null)
    {
        Title = habit is null ? "New habit" : "Edit habit";
        _name.Text = habit?.Name;
        _description.Text = habit?.Description;
        _repeat.SelectedItem = habit?.Schedule.RepeatType ?? "Daily";
        if (habit?.Schedule.OnceDate is DateOnly date)
            _onceDate.Date = date.ToDateTime(TimeOnly.MinValue);

        var days = Enum.GetValues<DayOfWeek>().OrderBy(day => day == DayOfWeek.Sunday ? 7 : (int)day);
        foreach (var day in days)
        {
            var check = new CheckBox { IsChecked = habit?.Schedule.DaysOfWeek?.Contains(day) == true };
            SemanticProperties.SetDescription(check, day.ToString());
            _checks.Add(day, check);
            var row = new HorizontalStackLayout { Spacing = 8 };
            row.Children.Add(check);
            row.Children.Add(new Label { Text = day.ToString(), VerticalOptions = LayoutOptions.Center });
            _weekdays.Children.Add(row);
        }

        _repeat.SelectedIndexChanged += (_, _) => ShowScheduleFields();
        ShowScheduleFields();
        Body.Children.Add(new Label { Text = "Habit name" });
        Body.Children.Add(_name);
        Body.Children.Add(new Label { Text = "Description" });
        Body.Children.Add(_description);
        Body.Children.Add(new Label { Text = "Schedule" });
        Body.Children.Add(_repeat);
        Body.Children.Add(_weekdays);
        Body.Children.Add(_onceLabel);
        Body.Children.Add(_onceDate);
        Body.Children.Add(HabitPresentation.Button("Save", () => RunAsync(async () =>
        {
            var schedule = BuildSchedule();
            if (habit is null)
                await client.CreateAsync(new CreateHabitRequest(_name.Text ?? "", _description.Text, schedule));
            else
                await client.UpdateAsync(habit.Id, new UpdateHabitRequest(_name.Text ?? "", _description.Text, schedule));

            await Navigation.PopAsync();
        })));
    }

    private void ShowScheduleFields()
    {
        _weekdays.IsVisible = _repeat.SelectedIndex == 1;
        _onceLabel.IsVisible = _onceDate.IsVisible = _repeat.SelectedIndex == 2;
    }

    private HabitScheduleDto BuildSchedule() => _repeat.SelectedIndex switch
    {
        1 => new("Weekly", _checks.Where(entry => entry.Value.IsChecked).Select(entry => entry.Key).ToArray()),
        2 => new("Once", OnceDate: DateOnly.FromDateTime(_onceDate.Date ?? DateTime.Today)),
        _ => new("Daily")
    };
}
