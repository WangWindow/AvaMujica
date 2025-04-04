using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AvaMujica.Models;

namespace AvaMujica.Services;

/// <summary>
/// 聊天服务，提供会话管理和消息处理功能
/// </summary>
/// <remarks>
/// 构造函数
/// </remarks>
public class ChatService()
{
    private readonly ConfigService _configService = ConfigService.Instance;
    private readonly ApiService _apiService = ApiService.Instance;
    private readonly DatabaseService _databaseService = DatabaseService.Instance;

    /// <summary>
    /// 单例实例
    /// </summary>
    private static ChatService? _instance;
    private static readonly Lock _lock = new();
    public static ChatService Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new ChatService();
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
    /// <param name="sessionId">会话ID</param>
    /// <returns>会话对象</returns>
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
    /// 创建新会话
    /// </summary>
    /// <param name="title">会话标题</param>
    /// <param name="type">会话类型</param>
    /// <returns>新创建的会话</returns>
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
    /// 更新会话信息
    /// </summary>
    /// <param name="session">要更新的会话</param>
    public async Task UpdateSessionAsync(ChatSession session)
    {
        await Task.Run(() =>
        {
            session.UpdatedTime = DateTime.Now;

            _databaseService.ExecuteNonQuery(
                "UPDATE ChatSessions SET Title = $title, Type = $type, UpdatedTime = $updatedTime WHERE Id = $id",
                new Dictionary<string, object>
                {
                    { "$id", session.Id },
                    { "$title", session.Title },
                    { "$type", session.Type },
                    { "$updatedTime", session.UpdatedTime },
                }
            );
        });
    }

    /// <summary>
    /// 删除会话
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    public async Task DeleteSessionAsync(string sessionId)
    {
        await Task.Run(() =>
        {
            // 删除会话的所有消息
            _databaseService.ExecuteNonQuery(
                "DELETE FROM ChatMessages WHERE SessionId = $sessionId",
                new Dictionary<string, object> { { "$sessionId", sessionId } }
            );

            // 删除会话
            _databaseService.ExecuteNonQuery(
                "DELETE FROM ChatSessions WHERE Id = $sessionId",
                new Dictionary<string, object> { { "$sessionId", sessionId } }
            );
        });
    }

    /// <summary>
    /// 添加消息到会话
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <param name="message">消息对象</param>
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
    /// 更新消息内容
    /// </summary>
    /// <param name="message">要更新的消息</param>
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
    /// 获取指定会话的所有消息
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <returns>消息列表</returns>
    public async Task<List<ChatMessage>> GetSessionMessagesAsync(string sessionId)
    {
        return await Task.Run(() =>
        {
            return GetSessionMessages(sessionId);
        });
    }

    /// <summary>
    /// 获取指定会话的最新一条助手消息
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <returns>最新的助手消息，如果不存在则返回null</returns>
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
    /// 发送消息并获取AI回复
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <param name="userContent">用户消息内容</param>
    /// <param name="onReceiveToken">接收令牌回调</param>
    /// <param name="onReceiveReasoning">接收推理内容回调</param>
    /// <returns>助手消息</returns>
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
                if (type == ResponseType.Content)
                {
                    // 更新内容
                    assistantMessage.Content += message;
                    onReceiveToken?.Invoke(message);

                    // 每收到一块内容就更新数据库
                    await UpdateMessageAsync(assistantMessage);
                }
                else if (type == ResponseType.ReasoningContent) // 使用预先获取的配置
                {
                    // 更新推理内容
                    assistantMessage.ReasoningContent += message;
                    onReceiveReasoning?.Invoke(message);

                    // 每收到一块推理内容就更新数据库
                    await UpdateMessageAsync(assistantMessage);
                }
            }
        );

        // 最后确保完全更新到数据库
        await UpdateMessageAsync(assistantMessage);

        return assistantMessage;
    }

    /// <summary>
    /// 获取当前配置
    /// </summary>
    /// <returns>配置对象</returns>
    public Config GetCurrentConfig() => _configService.LoadFullConfig();

    /// <summary>
    /// 保存配置
    /// </summary>
    /// <param name="config">配置对象</param>
    public void SaveConfig(Config config) => _configService.SaveFullConfig(config);
}
