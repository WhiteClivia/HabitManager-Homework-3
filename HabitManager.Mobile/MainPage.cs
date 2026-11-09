using HabitManager.Contracts;
using HabitManager.Mobile.Pages;
using HabitManager.Mobile.Services;

namespace HabitManager.Mobile;
//smth
public sealed class MainPage : ApiPage
{
    private readonly IHabitClient _client;

    private readonly Picker _filter = new() { Title = "Show habits", ItemsSource = new[] { "Today", "All active", "Archived" } };
    private readonly Label _progress = new() { FontAttributes = FontAttributes.Bold };
    private readonly VerticalStackLayout _list = new() { Spacing = 8 };
    private HabitDto[] _habits = [];

    public MainPage(IHabitClient client)
    {
        _client = client;
        Title = "Habit Manager";
        _filter.SelectedIndex = 0;
        _filter.SelectedIndexChanged += (_, _) => RenderHabits();
        Body.Children.Add(_filter);
        Body.Children.Add(HabitPresentation.Button("Refresh", ReloadAsync));
        Body.Children.Add(HabitPresentation.Button("Add habit", () => Navigation.PushAsync(new HabitEditorPage(_client))));
        Body.Children.Add(_progress);
        Body.Children.Add(_list);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ReloadAsync();
    }

    private Task ReloadAsync() => RunAsync(async () =>
    {
        _habits = await _client.ListAsync();
        RenderHabits();
    });

    private void RenderHabits()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var scheduledToday = _habits.Where(habit => !habit.IsArchived() &&
            habit.Schedule.ToDomain().IsScheduledFor(today)).ToArray();
        var visible = _filter.SelectedIndex switch
        {
            1 => _habits.Where(habit => !habit.IsArchived()).ToArray(),
            2 => _habits.Where(habit => habit.IsArchived()).ToArray(),
            _ => scheduledToday
        };

        _progress.Text = $"Today: {scheduledToday.Count(habit => habit.IsCompletedOn(today))}/{scheduledToday.Length} completed";
        _list.Children.Clear();
        if (visible.Length == 0)
            _list.Children.Add(new Label { Text = "No habits in this view." });

        foreach (var habit in visible)
        {
            var completed = habit.IsCompletedOn(today) ? " ✓" : "";
            _list.Children.Add(HabitPresentation.Button(habit.Name + completed,
                () => Navigation.PushAsync(new HabitDetailsPage(_client, habit.Id))));
        }
    }
}
