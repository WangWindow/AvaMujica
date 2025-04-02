using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AvaMujica.Models;

namespace AvaMujica.Services;

/// <summary>
/// 历史记录服务
/// </summary>
/// <remarks>
/// 构造函数
/// </remarks>
/// <param name="databaseService">数据库服务</param>
public class HistoryService(DatabaseService databaseService)
{
    /// <summary>
    /// 数据库服务
    /// </summary>
    private readonly DatabaseService _databaseService = databaseService;

    /// <summary>
    /// 获取会话历史记录
    /// </summary>
    /// <returns>按日期分组的历史记录</returns>
    public async Task<List<HistoryGroup>> GetHistoryGroupsAsync()
    {
        var historyItems = await _databaseService.GetHistoryInfoAsync();
        return GroupHistoryByDate(historyItems);
    }

    /// <summary>
    /// 按类型筛选会话历史记录
    /// </summary>
    /// <param name="type">会话类型</param>
    /// <returns>按日期分组的历史记录</returns>
    public async Task<List<HistoryGroup>> GetHistoryGroupsByTypeAsync(string type)
    {
        var historyItems = await _databaseService.GetHistoryInfoByTypeAsync(type);
        return GroupHistoryByDate(historyItems);
    }

    /// <summary>
    /// 将历史记录按日期分组
    /// </summary>
    private List<HistoryGroup> GroupHistoryByDate(List<HistoryInfo> historyItems)
    {
        var result = new List<HistoryGroup>();

        // 按日期分组
        var groupedHistory = historyItems
            .OrderByDescending(h => h.Time)
            .GroupBy(h => h.Time.Date)
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
