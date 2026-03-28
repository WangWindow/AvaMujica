using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using AvaMujica.Services;
using AvaMujica.ViewModels;
using AvaMujica.Views;
using Microsoft.Extensions.DependencyInjection;

namespace AvaMujica;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = default!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // 配置 DI
        ConfigureServices();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow { DataContext = Services.GetRequiredService<MainWindowViewModel>() };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView { DataContext = Services.GetRequiredService<MainViewModel>() };
        }

        var theme = Services.GetRequiredService<IConfigService>().LoadFullConfig().Theme;
        ApplyTheme(theme);

        base.OnFrameworkInitializationCompleted();
    }

    private static void ConfigureServices()
    {
        var services = new ServiceCollection();

        // Services
        services.AddSingleton<IDatabaseService, DatabaseService>();
        services.AddSingleton<IConfigService, ConfigService>();
        services.AddSingleton<IApiService, ApiService>();
        services.AddSingleton<IHistoryService, HistoryService>();

        // ViewModels
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MainWindowViewModel>();
        services.AddTransient<AssessmentViewModel>();
        services.AddTransient<PlanViewModel>();

        Services = services.BuildServiceProvider();
    }

    public static void ApplyTheme(string theme)
    {
        if (Current is null)
            return;

        var t = theme?.Trim()?.ToLowerInvariant();
        Current.RequestedThemeVariant = t switch
        {
            "light" => ThemeVariant.Light,
            "dark" => ThemeVariant.Dark,
            "system" => ThemeVariant.Default,
            _ => ThemeVariant.Default,
        };
    }
}
