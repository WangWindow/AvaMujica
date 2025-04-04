using AvaMujica.Services;
using CommunityToolkit.Mvvm.Input;

namespace AvaMujica.ViewModels;

/// <summary>
/// 设置视图模型
/// </summary>
public partial class SettingsViewModel(MainViewModel mainViewModel) : ViewModelBase
{
    private readonly ConfigService _configService = ConfigService.Instance;

    /// <summary>
    /// 主视图模型
    /// </summary>
    private readonly MainViewModel _mainViewModel = mainViewModel;

    /// <summary>
    /// 返回聊天界面
    /// </summary>
    [RelayCommand]
    private void GoBack()
    {
        _mainViewModel?.ReturnToChat();
    }
}
