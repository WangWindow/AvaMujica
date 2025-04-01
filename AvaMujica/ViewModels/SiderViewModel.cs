using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AvaMujica.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaMujica.ViewModels;

/// <summary>
/// 侧边栏视图模型
/// </summary>
public partial class SiderViewModel : ViewModelBase
{
    /// <summary>
    /// 主视图模型引用
    /// </summary>
    private readonly MainViewModel _mainViewModel;

    /// <summary>
    /// 历史记录分组集合（替代原来的GroupedHistoryItems属性）
    /// </summary>
    public ObservableCollection<HistoryGroup> HistoryGroups => _mainViewModel.HistoryGroups;

    /// <summary>
    /// 为保持兼容性，提供原属性名称
    /// </summary>
    public ObservableCollection<HistoryGroup> GroupedHistoryItems => HistoryGroups;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="mainViewModel">主视图模型</param>
    public SiderViewModel(MainViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;
    }

    /// <summary>
    /// 添加历史记录项
    /// </summary>
    /// <param name="historyInfo">历史记录信息</param>
    public void AddHistoryItem(HistoryInfo historyInfo)
    {
        _mainViewModel._historyManager.AddHistory(historyInfo);
    }

    /// <summary>
    /// 新建聊天命令
    /// </summary>
    [RelayCommand]
    public void CreateNewChat()
    {
        _mainViewModel.CreateNewChat();
    }

    /// <summary>
    /// 打开设置命令
    /// </summary>
    [RelayCommand]
    public void OpenSettings()
    {
        _mainViewModel.ShowSettings();
    }

    /// <summary>
    /// 清空历史记录命令
    /// </summary>
    [RelayCommand]
    public void ClearHistory()
    {
        // 确认是否清空
        // 此处可以添加确认对话框

        _mainViewModel._historyManager.ClearHistory();
    }
}
