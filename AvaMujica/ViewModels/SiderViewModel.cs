/*
 * @FilePath: SiderViewModel.cs
 * @Author: WangWindow 1598593280@qq.com
 * @Date: 2025-02-21 16:27:39
 * @LastEditors: WangWindow
 * @LastEditTime: 2025-02-26 17:47:04
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
    [ObservableProperty]
    private bool isOpen = false;

    public ObservableCollection<HistoryInfo> HistoryInfoList { get; } = [];

    /// <summary>
    /// 按时间段分组的历史记录项
    /// </summary>
    public ObservableCollection<HistoryGroup> GroupedHistoryItems { get; } = [];

    public SiderViewModel()
    {
        // 添加测试数据
        HistoryInfoList.Add(
            new HistoryInfo
            {
                Id = 1,
                Title = "测试 1",
                Time = DateTime.Now.AddHours(-1),
            }
        );

        HistoryInfoList.Add(
            new HistoryInfo
            {
                Id = 2,
                Title = "测试 2",
                Time = DateTime.Now.AddDays(-1),
            }
        );

        HistoryInfoList.Add(
            new HistoryInfo
            {
                Id = 3,
                Title = "测试 3",
                Time = DateTime.Now.AddDays(-2),
            }
        );

        HistoryInfoList.Add(
            new HistoryInfo
            {
                Id = 4,
                Title = "测试 4",
                Time = DateTime.Now.AddDays(-7),
            }
        );

        // 分组历史记录
        GroupHistoryItems();
    }

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
            .Select(g => new HistoryGroup(g.Key, g.ToList()));

        GroupedHistoryItems.Clear();
        foreach (var group in groupedItems)
        {
            GroupedHistoryItems.Add(group);
        }
    }

    /// <summary>
    /// 选择历史记录项的命令
    /// </summary>
    [RelayCommand]
    private void SelectHistory(HistoryInfo item)
    {
        // TODO: 实现选择历史记录项的逻辑
    }
}

/// <summary>
/// 历史记录分组类
/// </summary>
public class HistoryGroup
{
    public string Key { get; }
    public List<HistoryInfo> Items { get; }

    public HistoryGroup(string key, List<HistoryInfo> items)
    {
        Key = key;
        Items = items;
    }
}
