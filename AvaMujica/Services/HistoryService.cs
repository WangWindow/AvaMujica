using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AvaMujica.Models;

namespace AvaMujica.Services;

/// <summary>
/// 聊天历史记录服务，负责会话和消息管理
/// </summary>
public class HistoryService
{
    private readonly DatabaseService _databaseService = DatabaseService.Instance;
    private readonly ApiService _apiService = ApiService.Instance;

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
    /// 获取所有会话
    /// </summary>
    /// <returns>会话列表</returns>
    public async Task<List<ChatSession>> GetAllSessionsAsync()
    {
        return await Task.Run(() =>
        {
            var sessions = _databaseService.Query<ChatSession>(
                "SELECT Id, Title, Type, CreatedTime, UpdatedTime FROM ChatSessions ORDER BY UpdatedTime DESC",
                reader => new ChatSession
                {
                    Id = reader.GetString(0),
                    Title = reader.GetString(1),
                    Type = reader.GetString(2),
                    CreatedTime = reader.GetDateTime(3),
                    UpdatedTime = reader.GetDateTime(4),
                }
            );

            foreach (var session in sessions)
            {
                session.Messages = GetSessionMessages(session.Id);
            }

            return sessions;
        });
    }

    /// <summary>
    /// 获取指定会话
    /// </summary>
    public async Task<ChatSession> GetSessionAsync(string sessionId)
    {
        return await Task.Run(() =>
        {
            var sessions = _databaseService.Query<ChatSession>(
                "SELECT Id, Title, Type, CreatedTime, UpdatedTime FROM ChatSessions WHERE Id = $sessionId",
                reader => new ChatSession
                {
                    Id = reader.GetString(0),
                    Title = reader.GetString(1),
                    Type = reader.GetString(2),
                    CreatedTime = reader.GetDateTime(3),
                    UpdatedTime = reader.GetDateTime(4),
                },
                new Dictionary<string, object> { { "$sessionId", sessionId } }
            );

            if (sessions.Count == 0)
                throw new Exception($"找不到ID为 {sessionId} 的会话");

            var session = sessions[0];
            session.Messages = GetSessionMessages(sessionId);

            return session;
        });
    }

    /// <summary>
    /// 按类型筛选会话历史记录
    /// </summary>
    public async Task<List<ChatSessionGroup>> GetChatSessionHistorysByTypeAsync(string type)
    {
        var historyItems = await GetChatSessionByTypeAsync(type);
        return GroupHistoryByDate(historyItems);
    }

    /// <summary>
    /// 获取特定类型的历史记录信息
    /// </summary>
    public async Task<List<ChatSession>> GetChatSessionByTypeAsync(string type)
    {
        return await Task.Run(() =>
        {
            return _databaseService.Query<ChatSession>(
                "SELECT Id, Title, Type, CreatedTime, UpdatedTime FROM ChatSessions WHERE Type = $type ORDER BY UpdatedTime DESC",
                reader => new ChatSession
                {
                    Id = reader.GetString(0),
                    Title = reader.GetString(1),
                    Type = reader.GetString(2),
                    CreatedTime = reader.GetDateTime(3),
                    UpdatedTime = reader.GetDateTime(4),
                },
                new Dictionary<string, object> { { "$type", type } }
            );
        });
    }

    /// <summary>
    /// 创建新会话
    /// </summary>
    public async Task<ChatSession> CreateSessionAsync(string? title = null, string? type = null)
    {
        return await Task.Run(() =>
        {
            var session = new ChatSession(title, type);

            _databaseService.ExecuteNonQuery(
                "INSERT INTO ChatSessions (Id, Title, Type, CreatedTime, UpdatedTime) VALUES ($id, $title, $type, $createdTime, $updatedTime)",
                new Dictionary<string, object>
                {
                    { "$id", session.Id },
                    { "$title", session.Title },
                    { "$type", session.Type },
                    { "$createdTime", session.CreatedTime },
                    { "$updatedTime", session.UpdatedTime },
                }
            );

            return session;
        });
    }

    /// <summary>
    /// 获取指定会话的所有消息
    /// </summary>
    public async Task<List<ChatMessage>> GetSessionMessagesAsync(string sessionId)
    {
        return await Task.Run(() =>
        {
            return GetSessionMessages(sessionId);
        });
    }

    /// <summary>
    /// 内部方法：获取会话消息
    /// </summary>
    private List<ChatMessage> GetSessionMessages(string sessionId)
    {
        return _databaseService.Query<ChatMessage>(
            "SELECT Id, SessionId, Role, Content, ReasoningContent, SendTime FROM ChatMessages WHERE SessionId = $sessionId ORDER BY SendTime",
            reader => new ChatMessage
            {
                Id = reader.GetString(0),
                SessionId = reader.GetString(1),
                Role = reader.GetString(2),
                Content = reader.GetString(3),
                ReasoningContent = reader.IsDBNull(4) ? null : reader.GetString(4),
                SendTime = DateTime.Parse(reader.GetString(5)),
            },
            new Dictionary<string, object> { { "$sessionId", sessionId } }
        );
    }

    /// <summary>
    /// 添加消息到会话
    /// </summary>
    public async Task<ChatMessage> AddMessageAsync(string sessionId, ChatMessage message)
    {
        message.SessionId = sessionId;
        return await Task.Run(() =>
        {
            _databaseService.ExecuteNonQuery(
                "INSERT INTO ChatMessages (Id, SessionId, Role, Content, ReasoningContent, SendTime) "
                    + "VALUES ($id, $sessionId, $role, $content, $reasoningContent, $sendTime)",
                new Dictionary<string, object>
                {
                    { "$id", message.Id },
                    { "$sessionId", message.SessionId },
                    { "$role", message.Role },
                    { "$content", message.Content },
                    { "$reasoningContent", message.ReasoningContent as object ?? DBNull.Value },
                    { "$sendTime", message.SendTime.ToString("o") },
                }
            );

            // 更新会话的最后更新时间
            _databaseService.ExecuteNonQuery(
                "UPDATE ChatSessions SET UpdatedTime = $updatedTime WHERE Id = $sessionId",
                new Dictionary<string, object>
                {
                    { "$sessionId", message.SessionId },
                    { "$updatedTime", DateTime.Now.ToString("o") },
                }
            );

            return message;
        });
    }

    /// <summary>
    /// 更新消息到会话
    /// </summary>
    public async Task UpdateMessageAsync(ChatMessage message)
    {
        await Task.Run(() =>
        {
            _databaseService.ExecuteNonQuery(
                "UPDATE ChatMessages SET Content = $content, ReasoningContent = $reasoningContent WHERE Id = $id",
                new Dictionary<string, object>
                {
                    { "$id", message.Id },
                    { "$content", message.Content },
                    { "$reasoningContent", message.ReasoningContent as object ?? DBNull.Value },
                }
            );

            // 更新会话的最后更新时间
            _databaseService.ExecuteNonQuery(
                "UPDATE ChatSessions SET UpdatedTime = $updatedTime WHERE Id = $sessionId",
                new Dictionary<string, object>
                {
                    { "$sessionId", message.SessionId },
                    { "$updatedTime", DateTime.Now.ToString("o") },
                }
            );
        });
    }

    /// <summary>
    /// 更新会话标题
    /// </summary>
    public async Task UpdateSessionTitleAsync(string sessionId, string newTitle)
    {
        await Task.Run(() =>
        {
            _databaseService.ExecuteNonQuery(
                "UPDATE ChatSessions SET Title = $title, UpdatedTime = $updatedTime WHERE Id = $sessionId",
                new Dictionary<string, object>
                {
                    { "$sessionId", sessionId },
                    { "$title", newTitle },
                    { "$updatedTime", DateTime.Now.ToString("o") },
                }
            );
        });
    }

    /// <summary>
    /// 获取指定会话的最新一条助手消息
    /// </summary>
    public async Task<ChatMessage?> GetLatestAssistantMessageAsync(string sessionId)
    {
        return await Task.Run(() =>
        {
            var messages = _databaseService.Query<ChatMessage>(
                "SELECT * FROM ChatMessages WHERE SessionId = $sessionId AND Role = 'assistant' ORDER BY SendTime DESC LIMIT 1",
                reader => new ChatMessage
                {
                    Id = reader.GetString(0),
                    SessionId = reader.GetString(1),
                    Role = reader.GetString(2),
                    Content = reader.GetString(3),
                    ReasoningContent = reader.IsDBNull(4) ? null : reader.GetString(4),
                    SendTime = DateTime.Parse(reader.GetString(5)),
                },
                new Dictionary<string, object> { { "$sessionId", sessionId } }
            );

            return messages.Count > 0 ? messages[0] : null;
        });
    }

    /// <summary>
    /// 发送消息并获取AI回复
    /// </summary>
    public async Task<ChatMessage> SendMessageAsync(
        string sessionId,
        string userContent,
        Action<string>? onReceiveToken = null,
        Action<string>? onReceiveReasoning = null
    )
    {
        if (string.IsNullOrWhiteSpace(userContent))
        {
            throw new ArgumentException("消息内容不能为空", nameof(userContent));
        }

        // 添加用户消息
        var userMessage = ChatMessage.CreateUserMessage(sessionId, userContent);
        await AddMessageAsync(sessionId, userMessage);

        // 创建助手消息（初始为空）
        var assistantMessage = ChatMessage.CreateAssistantMessage(sessionId);

        // 先保存空消息到数据库
        await AddMessageAsync(sessionId, assistantMessage);

        // 调用API获取响应，并随时更新数据库
        await _apiService.ChatAsync(
            userContent,
            async (type, message) =>
            {
                await HandleApiResponse(
                    assistantMessage,
                    type,
                    message,
                    onReceiveToken,
                    onReceiveReasoning
                );
            }
        );

        return assistantMessage;
    }

    /// <summary>
    /// 处理API响应，更新消息内容并保存到数据库
    /// </summary>
    private async Task HandleApiResponse(
        ChatMessage assistantMessage,
        ResponseType type,
        string message,
        Action<string>? onReceiveToken = null,
        Action<string>? onReceiveReasoning = null
    )
    {
        if (type == ResponseType.Content)
        {
            // 更新内容
            assistantMessage.Content += message;
            onReceiveToken?.Invoke(message);
        }
        else if (type == ResponseType.ReasoningContent)
        {
            // 更新推理内容
            assistantMessage.ReasoningContent += message;
            onReceiveReasoning?.Invoke(message);
        }

        // 每收到一块内容就更新数据库
        await UpdateMessageAsync(assistantMessage);
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
