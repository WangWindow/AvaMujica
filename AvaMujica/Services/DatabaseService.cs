using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AvaMujica.Models;

namespace AvaMujica.Services;

/// <summary>
/// 数据库服务，提供会话和消息的数据操作
/// </summary>
/// <remarks>
/// 构造函数
/// </remarks>
/// <param name="database">SQLite数据库实例</param>
public class DatabaseService(SqliteDatabase database)
{
    /// <summary>
    /// 数据库实例
    /// </summary>
    private readonly SqliteDatabase _database = database;

    /// <summary>
    /// 获取所有会话
    /// </summary>
    /// <returns>会话列表</returns>
    public async Task<List<ChatSession>> GetAllSessionsAsync()
    {
        return await Task.Run(() =>
        {
            // 使用注入的数据库实例，而不是每次创建新的实例
            var sessions = _database.Query<ChatSession>(
                "SELECT * FROM ChatSessions ORDER BY UpdatedAt DESC",
                reader => new ChatSession
                {
                    Id = reader.GetString(0),
                    Title = reader.GetString(1),
                    Type = reader.GetString(2),
                    CreatedAt = DateTime.Parse(reader.GetString(3)),
                    UpdatedAt = DateTime.Parse(reader.GetString(4)),
                }
            );

            // 加载每个会话的消息
            foreach (var session in sessions)
            {
                session.Messages = GetSessionMessages(_database, session.Id);
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
            // 使用注入的数据库实例
            var sessions = _database.Query<ChatSession>(
                "SELECT * FROM ChatSessions WHERE Id = $sessionId",
                reader => new ChatSession
                {
                    Id = reader.GetString(0),
                    Title = reader.GetString(1),
                    Type = reader.GetString(2),
                    CreatedAt = DateTime.Parse(reader.GetString(3)),
                    UpdatedAt = DateTime.Parse(reader.GetString(4)),
                },
                new Dictionary<string, object> { { "$sessionId", sessionId } }
            );

            if (sessions.Count == 0)
                throw new Exception($"找不到ID为 {sessionId} 的会话");

            var session = sessions[0];

            // 加载会话消息
            session.Messages = GetSessionMessages(_database, sessionId);

            return session;
        });
    }

    /// <summary>
    /// 创建新会话
    /// </summary>
    /// <param name="title">会话标题</param>
    /// <param name="type">会话类型</param>
    /// <returns>新创建的会话</returns>
    public async Task<ChatSession> CreateSessionAsync(string title, string type)
    {
        return await Task.Run(() =>
        {
            var session = new ChatSession { Title = title, Type = type };

            _database.ExecuteNonQuery(
                "INSERT INTO ChatSessions (Id, Title, Type, CreatedAt, UpdatedAt) VALUES ($id, $title, $type, $createdAt, $updatedAt)",
                new Dictionary<string, object>
                {
                    { "$id", session.Id },
                    { "$title", session.Title },
                    { "$type", session.Type },
                    { "$createdAt", session.CreatedAt.ToString("o") },
                    { "$updatedAt", session.UpdatedAt.ToString("o") },
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
            session.UpdatedAt = DateTime.Now;

            _database.ExecuteNonQuery(
                "UPDATE ChatSessions SET Title = $title, Type = $type, UpdatedAt = $updatedAt WHERE Id = $id",
                new Dictionary<string, object>
                {
                    { "$id", session.Id },
                    { "$title", session.Title },
                    { "$type", session.Type },
                    { "$updatedAt", session.UpdatedAt.ToString("o") },
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
            _database.ExecuteNonQuery(
                "DELETE FROM ChatMessages WHERE SessionId = $sessionId",
                new Dictionary<string, object> { { "$sessionId", sessionId } }
            );

            // 删除会话
            _database.ExecuteNonQuery(
                "DELETE FROM ChatSessions WHERE Id = $sessionId",
                new Dictionary<string, object> { { "$sessionId", sessionId } }
            );
        });
    }

    /// <summary>
    /// 添加消息到会话
    /// </summary>
    /// <param name="message">消息对象</param>
    public async Task<ChatMessage> AddMessageAsync(ChatMessage message)
    {
        return await Task.Run(() =>
        {
            _database.ExecuteNonQuery(
                "INSERT INTO ChatMessages (Id, SessionId, Role, Content, ReasoningContent, Time) "
                    + "VALUES ($id, $sessionId, $role, $content, $reasoningContent, $time)",
                new Dictionary<string, object>
                {
                    { "$id", message.Id },
                    { "$sessionId", message.SessionId },
                    { "$role", message.Role },
                    { "$content", message.Content },
                    { "$reasoningContent", message.ReasoningContent as object ?? DBNull.Value },
                    { "$time", message.Time.ToString("o") },
                }
            );

            // 更新会话的最后更新时间
            _database.ExecuteNonQuery(
                "UPDATE ChatSessions SET UpdatedAt = $updatedAt WHERE Id = $sessionId",
                new Dictionary<string, object>
                {
                    { "$sessionId", message.SessionId },
                    { "$updatedAt", DateTime.Now.ToString("o") },
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
            _database.ExecuteNonQuery(
                "UPDATE ChatMessages SET Content = $content, ReasoningContent = $reasoningContent WHERE Id = $id",
                new Dictionary<string, object>
                {
                    { "$id", message.Id },
                    { "$content", message.Content },
                    { "$reasoningContent", message.ReasoningContent as object ?? DBNull.Value },
                }
            );

            // 更新会话的最后更新时间
            _database.ExecuteNonQuery(
                "UPDATE ChatSessions SET UpdatedAt = $updatedAt WHERE Id = $sessionId",
                new Dictionary<string, object>
                {
                    { "$sessionId", message.SessionId },
                    { "$updatedAt", DateTime.Now.ToString("o") },
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
            return GetSessionMessages(_database, sessionId);
        });
    }

    /// <summary>
    /// 获取指定会话的最新一条助手消息
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <returns>最新的助手消息</returns>
    public async Task<ChatMessage?> GetLatestAssistantMessageAsync(string sessionId)
    {
        return await Task.Run(() =>
        {
            var messages = _database.Query<ChatMessage>(
                "SELECT * FROM ChatMessages WHERE SessionId = $sessionId AND Role = 'assistant' ORDER BY Time DESC LIMIT 1",
                reader => new ChatMessage
                {
                    Id = reader.GetString(0),
                    SessionId = reader.GetString(1),
                    Role = reader.GetString(2),
                    Content = reader.GetString(3),
                    ReasoningContent = reader.IsDBNull(4) ? null : reader.GetString(4),
                    Time = DateTime.Parse(reader.GetString(5)),
                },
                new Dictionary<string, object> { { "$sessionId", sessionId } }
            );

            return messages.Count > 0 ? messages[0] : null;
        });
    }

    /// <summary>
    /// 获取历史记录信息
    /// </summary>
    /// <returns>历史记录信息</returns>
    public async Task<List<HistoryInfo>> GetHistoryInfoAsync()
    {
        return await Task.Run(() =>
        {
            return _database.Query<HistoryInfo>(
                "SELECT Id, Title, CreatedAt as Time, Type FROM ChatSessions ORDER BY UpdatedAt DESC",
                reader => new HistoryInfo
                {
                    Id = reader.GetString(0),
                    Title = reader.GetString(1),
                    Time = DateTime.Parse(reader.GetString(2)),
                    Type = reader.GetString(3),
                }
            );
        });
    }

    /// <summary>
    /// 获取特定类型的历史记录信息
    /// </summary>
    /// <param name="type">会话类型</param>
    /// <returns>历史记录信息</returns>
    public async Task<List<HistoryInfo>> GetHistoryInfoByTypeAsync(string type)
    {
        return await Task.Run(() =>
        {
            return _database.Query<HistoryInfo>(
                "SELECT Id, Title, CreatedAt as Time, Type FROM ChatSessions WHERE Type = $type ORDER BY UpdatedAt DESC",
                reader => new HistoryInfo
                {
                    Id = reader.GetString(0),
                    Title = reader.GetString(1),
                    Time = DateTime.Parse(reader.GetString(2)),
                    Type = reader.GetString(3),
                },
                new Dictionary<string, object> { { "$type", type } }
            );
        });
    }

    /// <summary>
    /// 内部方法：获取会话消息
    /// </summary>
    private List<ChatMessage> GetSessionMessages(SqliteDatabase db, string sessionId)
    {
        return db.Query<ChatMessage>(
            "SELECT * FROM ChatMessages WHERE SessionId = $sessionId ORDER BY Time",
            reader => new ChatMessage
            {
                Id = reader.GetString(0),
                SessionId = reader.GetString(1),
                Role = reader.GetString(2),
                Content = reader.GetString(3),
                ReasoningContent = reader.IsDBNull(4) ? null : reader.GetString(4),
                Time = DateTime.Parse(reader.GetString(5)),
            },
            new Dictionary<string, object> { { "$sessionId", sessionId } }
        );
    }
}
