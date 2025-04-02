using System;
using System.Collections.Generic;
using AvaMujica.ViewModels;

namespace AvaMujica.Models;

/// <summary>
/// 历史记录分组类
/// </summary>
public class HistoryGroup(string key, List<HistoryInfo> items)
{
    /// <summary>
    /// 分组的键
    /// </summary>
    public string Key { get; } = key;

    /// <summary>
    /// 分组的历史记录信息
    /// </summary>
    public List<HistoryInfo> Items { get; } = items;
}
