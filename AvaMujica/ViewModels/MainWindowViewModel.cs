namespace AvaMujica.ViewModels;

/// <summary>
/// 主视图模型
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    public MainViewModel MainViewModel { get; }

    public MainWindowViewModel(MainViewModel mainViewModel)
    {
        MainViewModel = mainViewModel;
    }
}