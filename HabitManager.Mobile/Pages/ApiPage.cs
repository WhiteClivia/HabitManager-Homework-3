using HabitManager.Mobile.Services;

namespace HabitManager.Mobile.Pages;

public abstract class ApiPage : ContentPage
{
    private readonly Label _status = new() { IsVisible = false };
    private readonly ActivityIndicator _activity = new() { IsVisible = false };
    protected VerticalStackLayout Body { get; } = new() { Spacing = 12 };

    protected ApiPage()
    {
        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = new Thickness(20),
                Spacing = 12,
                Children = { _status, _activity, Body }
            }
        };
    }

    protected async Task RunAsync(Func<Task> action)
    {
        if (_activity.IsRunning)
            return;

        _status.IsVisible = false;
        _activity.IsRunning = _activity.IsVisible = true;
        Body.IsEnabled = false;
        try
        {
            await action();
        }
        catch (HabitClientException error)
        {
            _status.Text = error.Message;
            _status.IsVisible = true;
        }
        finally
        {
            Body.IsEnabled = true;
            _activity.IsRunning = _activity.IsVisible = false;
        }
    }
}
