using System;
using System.Collections.Generic;
using AvaMujica.ViewModels;

namespace AvaMujica.Models;

/// <summary>
/// 历史记录信息模型
/// </summary>
public class HistoryInfo
{
    /// <summary>
    /// ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 时间
    /// </summary>
    public DateTime Time { get; set; }

    /// <summary>
    /// 会话类型
    /// </summary>
    public string Type { get; set; } = "咨询";

    /// <summary>
    /// 关联的聊天视图模型
    /// </summary>
    public ChatViewModel? ChatViewModel { get; set; }

    /// <summary>
    /// 格式化后的时间显示
    /// </summary>
    public string FormattedTime => Time.ToString("HH:mm");
}

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
