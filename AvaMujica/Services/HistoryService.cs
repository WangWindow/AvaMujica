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
public class HistoryService(IDatabaseService databaseService, IApiService apiService) : IHistoryService
{
    private readonly IDatabaseService _databaseService = databaseService;
    private readonly IApiService _apiService = apiService;

    /// <summary>
    /// 获取所有会话
    /// </summary>
    /// <returns>会话列表</returns>
    public async Task<List<ChatSession>> GetAllSessionsAsync()
    {
        return await Task.Run(() =>
        {
            var sessions = _databaseService.Query(
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
    public async Task<ChatSession?> GetSessionAsync(string sessionId)
    {
        return await Task.Run(() =>
        {
            var sessions = _databaseService.Query(
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
                return null;

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
            return _databaseService.Query(
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
            var session = new ChatSession
            {
                Title = title ?? "新会话",
                Type = type ?? SessionType.Chat,
            };

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
        return _databaseService.Query(
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

    // 显式接口实现，兼容接口返回 Task
    Task IHistoryService.AddMessageAsync(string sessionId, ChatMessage message)
    {
        return AddMessageAsync(sessionId, message);
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
            var messages = _databaseService.Query(
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
    /// 发送一条消息（带历史上下文），流式回调 onDelta，返回用户与助手消息。
    /// </summary>
    public async Task<(ChatMessage userMessage, ChatMessage assistantMessage)> SendMessageAsync(
        string sessionId,
        string userContent,
        Func<ResponseType, string, Task> onDelta,
    Action<ChatMessage, ChatMessage>? onMessagesCreated = null,
        CancellationToken cancellationToken = default,
        Action<Exception>? onError = null)
    {
        if (string.IsNullOrWhiteSpace(userContent))
            throw new ArgumentException("消息内容不能为空", nameof(userContent));

        // 1. 持久化用户消息
        var userMessage = ChatMessage.CreateUserMessage(sessionId, userContent);
        await AddMessageAsync(sessionId, userMessage);

        // 2. 创建并保存空助手消息（reasoning/content 为空，UI 可立即显示）
        var assistantMessage = ChatMessage.CreateAssistantMessage(sessionId, string.Empty, string.Empty);
        await AddMessageAsync(sessionId, assistantMessage);

        // UI 先获引用，便于立即插入列表显示占位
        onMessagesCreated?.Invoke(userMessage, assistantMessage);

        // 3. 构造历史上下文（排除本轮新建的 user 与空 assistant）
        var existing = await GetSessionMessagesAsync(sessionId);
        var historyList = new List<(string role, string content, string? reasoningContent)>();
        foreach (var m in existing)
        {
            if (m.Id == userMessage.Id || m.Id == assistantMessage.Id) continue;
            historyList.Add((m.Role, m.Content, m.ReasoningContent));
        }

        // 4. 处理要发送给模型的本轮 prompt：除第一轮外追加 <think> 规范提示；不写入历史，仅对模型可见
        string promptToSend = historyList.Count > 0
            ? BuildThinkEnforcedPrompt(userContent)
            : userContent;

        // 5. 调用底层 API（支持 cancellation / error）
        await _apiService.ChatAsync(
            promptToSend,
            async (type, delta) =>
            {
                if (type == ResponseType.Content)
                {
                    assistantMessage.Content += delta;
                }
                else if (type == ResponseType.ReasoningContent)
                {
                    assistantMessage.ReasoningContent += delta;
                }

                await UpdateMessageAsync(assistantMessage);
                await onDelta(type, delta);
            },
            historyList,
            cancellationToken,
            ex => onError?.Invoke(ex)
        );

        // Fallback：若 ReasoningContent 仍为空但 Content 中包含 <think>...</think>，尝试一次性拆分
        if (string.IsNullOrEmpty(assistantMessage.ReasoningContent) && assistantMessage.Content.Contains("<think>", StringComparison.OrdinalIgnoreCase))
        {
            var content = assistantMessage.Content;
            int start = content.IndexOf("<think>", StringComparison.OrdinalIgnoreCase);
            int end = content.IndexOf("</think>", StringComparison.OrdinalIgnoreCase);
            if (start >= 0 && end > start)
            {
                int innerStart = start + 7;
                var reasoning = content.Substring(innerStart, end - innerStart);
                var after = content.Substring(end + 8);
                assistantMessage.ReasoningContent = reasoning;
                assistantMessage.Content = after;
                await UpdateMessageAsync(assistantMessage);
            }
        }

        return (userMessage, assistantMessage);
    }

    /// <summary>
    /// 构造带强制 <think> 指令的提示词（不会写入历史，仅本轮发送给模型）
    /// </summary>
    private static string BuildThinkEnforcedPrompt(string original)
    {
        return "你必须在<think>和</think>标签内给出你的推理过程，然后，在</think>标签后给出最终的答案。\n\n以下是我的问题：\n" + original;
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
