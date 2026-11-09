using HabitManager.Mobile.Services;
using Microsoft.Extensions.Logging;


namespace HabitManager.Mobile
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            var host = DeviceInfo.Platform == DevicePlatform.Android ? "10.0.2.2" : "localhost";
            builder.Services.AddSingleton(_ => new HttpClient
            {
                BaseAddress = new Uri($"http://{host}:5187/"),
                Timeout = TimeSpan.FromSeconds(10)
            });
            builder.Services.AddSingleton<IHabitClient, HabitApiClient>();
            builder.Services.AddTransient<MainPage>();

            return builder.Build();
        }
    }
}

