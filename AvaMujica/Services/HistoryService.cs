using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AvaMujica.Models;

namespace AvaMujica.Services;

/// <summary>
/// 历史记录服务
/// </summary>
public class HistoryService
{
    private readonly DatabaseService _databaseService = DatabaseService.Instance;

    /// <summary>
    /// 单例实例
    /// </summary>
    private static HistoryService? _instance;
    private static readonly Lock _lock = new();
    public static HistoryService Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new HistoryService();
                }
            }
            return _instance;
        }
    }

    /// <summary>
    /// 按类型筛选会话历史记录
    /// </summary>
    /// <param name="type">会话类型</param>
    /// <returns>按日期分组的历史记录</returns>
    public async Task<List<ChatSessionGroup>> GetChatSessionHistorysByTypeAsync(string type)
    {
        var historyItems = await GetChatSessionByTypeAsync(type);
        return GroupHistoryByDate(historyItems);
    }

    /// <summary>
    /// 获取历史记录信息
    /// </summary>
    /// <returns>历史记录信息</returns>
    public async Task<List<ChatSession>> GetChatSessionAsync()
    {
        return await Task.Run(() =>
        {
            return _databaseService.Query<ChatSession>(
                "SELECT Id, Title, Type, CreatedTime FROM ChatSessions ORDER BY UpdatedTime DESC",
                reader => new ChatSession
                {
                    Id = reader.GetString(0),
                    Title = reader.GetString(1),
                    Type = reader.GetString(2),
                    CreatedTime = reader.GetDateTime(3),
                }
            );
        });
    }

    /// <summary>
    /// 获取特定类型的历史记录信息
    /// </summary>
    /// <param name="type">会话类型</param>
    /// <returns>历史记录信息</returns>
    public async Task<List<ChatSession>> GetChatSessionByTypeAsync(string type)
    {
        // 直接使用类型名称进行查询，不需要转换
        return await Task.Run(() =>
        {
            return _databaseService.Query<ChatSession>(
                "SELECT Id, Title, Type, CreatedTime FROM ChatSessions WHERE Type = $type ORDER BY UpdatedTime DESC",
                reader => new ChatSession
                {
                    Id = reader.GetString(0),
                    Title = reader.GetString(1),
                    Type = reader.GetString(2),
                    CreatedTime = reader.GetDateTime(3),
                },
                new Dictionary<string, object> { { "$type", type } }
            );
        });
    }

    /// <summary>
    /// 将历史记录按日期分组
    /// </summary>
    private List<ChatSessionGroup> GroupHistoryByDate(List<ChatSession> historyItems)
    {
        var result = new List<ChatSessionGroup>();

        // 按日期分组
        var groupedHistory = historyItems
            .OrderByDescending(h => h.CreatedTime)
            .GroupBy(h => h.CreatedTime.Date)
            .Select(g => new { Date = g.Key, Items = g.ToList() });

        // 转换为ChatSessionGroup对象
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

            result.Add(new ChatSessionGroup(groupKey, group.Items));
        }

        return result;
    }
}
