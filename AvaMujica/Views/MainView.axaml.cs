using Avalonia.Controls;
using Avalonia.Input;
using AvaMujica.ViewModels;

namespace AvaMujica.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        this.KeyUp += OnKeyUp;
    }

    private void OnKeyUp(object? sender, KeyEventArgs e)
    {
        if (DataContext is not MainViewModel vm) return;
        if (e.Key == Key.Escape || e.Key == Key.Back)
        {
            vm.BackCommand.Execute(null);
        }
        else if (e.Key == Key.Left)
        {
            vm.PreviousTabCommand.Execute(null);
        }
        else if (e.Key == Key.Right)
        {
            vm.NextTabCommand.Execute(null);
        }
    }
}
