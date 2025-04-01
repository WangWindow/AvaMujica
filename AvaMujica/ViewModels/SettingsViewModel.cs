using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AvaMujica.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaMujica.ViewModels;

/// <summary>
/// 设置视图模型
/// </summary>
public partial class SettingsViewModel : ViewModelBase
{
    /// <summary>
    /// 主视图模型
    /// </summary>
    private readonly MainViewModel _mainViewModel;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="mainViewModel"></param>
    public SettingsViewModel(MainViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;
    }

    // 无参构造函数，用于XAML设计时
    public SettingsViewModel()
        : this(null!) { }

    /// <summary>
    /// 返回聊天界面
    /// </summary>
    [RelayCommand]
    private void GoBack()
    {
        _mainViewModel?.ReturnToChat();
    }
}
