using System;
using System.Collections.Generic;
using System.Linq;
using AvaMujica.ViewModels;

namespace AvaMujica.Models;

/// <summary>
/// 历史记录管理器
/// </summary>
public class HistoryManager
{
    /// <summary>
    /// 历史记录列表
    /// </summary>
    private readonly List<HistoryInfo> _historyList = [];

    /// <summary>
    /// 添加历史记录
    /// </summary>
    /// <param name="historyInfo">历史记录信息</param>
    public void AddHistory(HistoryInfo historyInfo)
    {
        // 检查是否已存在相同ID的历史记录
        var existingItem = _historyList.FirstOrDefault(h => h.Id == historyInfo.Id);
        if (existingItem != null)
        {
            // 已存在则更新
            existingItem.Title = historyInfo.Title;
            existingItem.Time = historyInfo.Time;
            existingItem.ChatViewModel = historyInfo.ChatViewModel;
        }
        else
        {
            // 不存在则添加
            _historyList.Add(historyInfo);
        }
    }

    /// <summary>
    /// 删除历史记录
    /// </summary>
    /// <param name="id">历史记录ID</param>
    public void RemoveHistory(string id)
    {
        _historyList.RemoveAll(h => h.Id == id);
    }

    /// <summary>
    /// 清空历史记录
    /// </summary>
    public void ClearHistory()
    {
        _historyList.Clear();
    }

    /// <summary>
    /// 获取所有历史记录
    /// </summary>
    /// <returns>历史记录列表</returns>
    public List<HistoryInfo> GetHistory()
    {
        return _historyList;
    }

    /// <summary>
    /// 获取按日期分组的历史记录
    /// </summary>
    /// <returns>按日期分组的历史记录</returns>
    public List<HistoryGroup> GetHistoryByDateGroups()
    {
        var result = new List<HistoryGroup>();

        // 按日期分组
        var groupedHistory = _historyList
            .OrderByDescending(h => h.Time) // 按时间降序排列
            .GroupBy(h => h.Time.Date) // 按日期分组
            .Select(g => new { Date = g.Key, Items = g.ToList() });

        // 转换为HistoryGroup对象
        foreach (var group in groupedHistory)
        {
            string groupKey;
            if (group.Date == DateTime.Today)
            {
                groupKey = "今天";
            }
            else if (group.Date == DateTime.Today.AddDays(-1))
            {
                groupKey = "昨天";
            }
            else if (group.Date > DateTime.Today.AddDays(-7))
            {
                groupKey = "本周";
            }
            else if (group.Date.Year == DateTime.Today.Year)
            {
                groupKey = $"{group.Date.Month}月";
            }
            else
            {
                groupKey = group.Date.ToString("yyyy年M月");
            }

            result.Add(new HistoryGroup(groupKey, group.Items));
        }

        return result;
    }
}

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
