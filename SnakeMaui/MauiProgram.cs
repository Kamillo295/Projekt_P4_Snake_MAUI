using Microsoft.Extensions.Logging;
using SnakeMaui.Models;
using SnakeMaui.Services.Implementations;
using SnakeMaui.Services.Interfaces;
using SnakeMaui.ViewModels;

namespace SnakeMaui
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

            builder.Services.AddSingleton(new GameOptions
            {
                BoardSize = 24,
                StartLength = 20,
                PointsPerFood = 10
            });

            builder.Services.AddSingleton<IRandomProvider, RandomProvider>();
            builder.Services.AddSingleton<IGameService, GameService>();
            builder.Services.AddSingleton<IGameClock, MauiGameClock>();
            builder.Services.AddSingleton<IScoreRepository, JsonScoreRepository>();
            builder.Services.AddSingleton<MainPageViewModel>();
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddSingleton<AppShell>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
