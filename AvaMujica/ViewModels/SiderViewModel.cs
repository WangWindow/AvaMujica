/*
 * @FilePath: SiderViewModel.cs
 * @Author: WangWindow 1598593280@qq.com
 * @Date: 2025-02-21 16:27:39
 * @LastEditors: WangWindow
 * @LastEditTime: 2025-02-28 00:35:56
 * 2025 by WangWindow, All Rights Reserved.
 * @Description:
 */
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
    /// 侧边栏是否打开(默认关闭)
    /// </summary>
    [ObservableProperty]
    private bool isOpen = false;

    /// <summary>
    /// 主视图模型
    /// </summary>
    private readonly MainViewModel _mainViewModel;

    /// <summary>
    /// 历史记录信息列表
    /// </summary>
    public ObservableCollection<HistoryInfo> HistoryInfoList { get; } = [];

    /// <summary>
    /// 按时间段分组的历史记录项
    /// </summary>
    public ObservableCollection<HistoryGroup> GroupedHistoryItems { get; } = [];

    public SiderViewModel(MainViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;

        // 分组历史记录
        GroupHistoryItems();
    }

    // 无参构造函数，用于XAML设计时
    public SiderViewModel()
        : this(null!) { }

    /// <summary>
    /// 按时间段对历史记录进行分组
    /// </summary>
    private void GroupHistoryItems()
    {
        var today = DateTime.Today;
        var yesterday = today.AddDays(-1);
        var lastWeek = today.AddDays(-7);

        // 按时间段分组
        var groupedItems = HistoryInfoList
            .OrderByDescending(item => item.Time)
            .GroupBy(item =>
            {
                var itemDate = item.Time.Date;
                if (itemDate == today)
                    return "今天";
                else if (itemDate == yesterday)
                    return "昨天";
                else if (itemDate >= lastWeek)
                    return "最近一周";
                else
                    return "更早";
            })
            .Select(g => new HistoryGroup(g.Key, [.. g]));

        GroupedHistoryItems.Clear();
        foreach (var group in groupedItems)
        {
            GroupedHistoryItems.Add(group);
        }
    }

    /// <summary>
    /// 添加历史记录项
    /// </summary>
    public void AddHistoryItem(HistoryInfo item)
    {
        // 添加到历史记录列表
        HistoryInfoList.Add(item);

        // 重新分组历史记录
        GroupHistoryItems();
    }

    /// <summary>
    /// 选择历史记录项的命令
    /// </summary>
    [RelayCommand]
    private void SelectHistory(HistoryInfo item)
    {
        if (item?.ChatViewModel != null)
        {
            // 切换到选中的对话
            _mainViewModel.SwitchChat(item.ChatViewModel);

            // 关闭侧边栏
            _mainViewModel.IsSiderOpen = false;
        }
    }

    /// <summary>
    /// 跳转到设置页面
    /// </summary>
    [RelayCommand]
    private void OpenSettings()
    {
        _mainViewModel?.ShowSettings();
    }
}
