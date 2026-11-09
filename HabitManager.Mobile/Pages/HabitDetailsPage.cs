using HabitManager.Contracts;
using HabitManager.Mobile.Services;

namespace HabitManager.Mobile.Pages;

internal sealed class HabitDetailsPage : ApiPage
{
    private readonly IHabitClient _client;
    private readonly Guid _id;
    private readonly Label _details = new();
    private readonly Label _history = new();
    private readonly Label _dayStatus = new();
    private readonly DatePicker _date = new() { Date = DateTime.Today };
    private readonly Button _toggle;
    private readonly Button _edit;
    private readonly Button _archive;
    private HabitDto? _habit;

    public HabitDetailsPage(IHabitClient client, Guid id)
    {
        _client = client;
        _id = id;
        Title = "Habit details";
        _toggle = HabitPresentation.Button("Mark completed", ToggleCompletionAsync);
        _edit = HabitPresentation.Button("Edit", () => Navigation.PushAsync(new HabitEditorPage(_client, _habit)));
        _archive = HabitPresentation.Button("Archive", ToggleArchivedAsync);
        _date.DateSelected += (_, _) => RenderDay();

        Body.Children.Add(HabitPresentation.Button("Refresh", ReloadAsync));
        Body.Children.Add(_details);
        Body.Children.Add(_edit);
        Body.Children.Add(_archive);
        Body.Children.Add(new Label { Text = "Completion date", FontAttributes = FontAttributes.Bold });
        Body.Children.Add(_date);
        Body.Children.Add(_dayStatus);
        Body.Children.Add(_toggle);
        Body.Children.Add(_history);
        _edit.IsEnabled = _archive.IsEnabled = _toggle.IsEnabled = false;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ReloadAsync();
    }

    private DateOnly SelectedDate => DateOnly.FromDateTime(_date.Date ?? DateTime.Today);

    private Task ReloadAsync() => RunAsync(LoadAsync);

    private async Task LoadAsync()
    {
        var habit = await _client.GetAsync(_id);
        _habit = habit;
        _details.Text = $"{habit.Name}\n{habit.Description}\n{habit.Schedule.ToDomain()}\nStatus: {habit.Status}";
        _edit.IsEnabled = !habit.IsArchived();
        _archive.IsEnabled = true;
        _archive.Text = habit.IsArchived() ? "Restore from archive" : "Archive";
        _history.Text = habit.Completions.Count == 0
            ? "No completion history."
            : "Completion history:\n" + string.Join("\n", habit.Completions.Select(item => item.Date.ToString("d")));
        RenderDay();
    }

    private void RenderDay()
    {
        if (_habit is not { } habit)
            return;

        var completed = habit.IsCompletedOn(SelectedDate);
        var scheduled = habit.Schedule.ToDomain().IsScheduledFor(SelectedDate);
        _dayStatus.Text = completed ? "Completed" : scheduled ? "Scheduled" : "Not scheduled";
        _toggle.Text = completed ? "Undo completion" : "Mark completed";
        _toggle.IsEnabled = !habit.IsArchived() && (completed || scheduled);
    }

    private Task ToggleCompletionAsync() => RunAsync(async () =>
    {
        if (_habit is { } habit)
        {
            var date = SelectedDate;
            await _client.SetCompletedAsync(_id, date, !habit.IsCompletedOn(date));
            await LoadAsync();
        }
    });

    private Task ToggleArchivedAsync() => RunAsync(async () =>
    {
        if (_habit is { } habit)
        {
            await _client.SetArchivedAsync(_id, !habit.IsArchived());
            await LoadAsync();
        }
    });
}
