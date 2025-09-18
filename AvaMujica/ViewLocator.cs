using Avalonia.Controls;
using Avalonia.Controls.Templates;
using AvaMujica.ViewModels;
using AvaMujica.Views;

namespace AvaMujica;

public class ViewLocator : IDataTemplate
{
    public Control? Build(object? param)
    {
        if (param is null)
            return null;

        return param switch
        {
            AssessmentViewModel => new AssessmentView(),
            PlanViewModel => new PlanView(),
            ChatViewModel => new ChatView(),
            SettingsViewModel => new SettingsView(),
            SiderViewModel => new SiderView(),
            MainWindowViewModel => new MainWindow(),
            MainViewModel => new MainView(),
            _ => new TextBlock { Text = "Not Found: " + param.GetType().FullName }
        };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}
