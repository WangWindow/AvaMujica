using Microsoft.Extensions.DependencyInjection;

namespace AvaMujica.ViewModels;

public partial class MainWindowViewModel(MainViewModel mainViewModel) : ViewModelBase
{
    public MainWindowViewModel()
    : this(App.Services.GetRequiredService<MainViewModel>()) { }

    public MainViewModel MainViewModel { get; } = mainViewModel;
}