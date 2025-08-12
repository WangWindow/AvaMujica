namespace AvaMujica.ViewModels;

public partial class MainWindowViewModel(MainViewModel mainViewModel) : ViewModelBase
{
    public MainViewModel MainViewModel { get; } = mainViewModel;
}