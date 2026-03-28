using Avalonia.Controls;
using Avalonia.Controls.Templates;
using AvaMujica.ViewModels;
using AvaMujica.Views;

namespace AvaMujica;

/// <summary>
/// Explicit ViewModel → View mapping to avoid name-based reflection lookups.
/// Register mappings here so the application can create views for known viewmodels.
/// </summary>
public class ViewLocator : IDataTemplate
{
    public Control? Build(object? param) => param switch
    {
        null => null,
        AssessmentViewModel => new AssessmentView(),
        ChatViewModel => new ChatView(),
        MainViewModel => new MainView(),
        MainWindowViewModel => new MainWindow(),
        PlanViewModel => new PlanView(),
        SettingsViewModel => new SettingsView(),
        SiderViewModel => new SiderView(),
        _ => new TextBlock { Text = $"View not implemented for: {param.GetType().FullName}" }
    };

    public bool Match(object? data) => data is ViewModelBase;
}
