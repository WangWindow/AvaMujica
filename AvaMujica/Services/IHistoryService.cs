using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AvaMujica.Models;

namespace AvaMujica.Services;

public interface IHistoryService
{
    Task<List<ChatSession>> GetAllSessionsAsync();
    Task<ChatSession?> GetSessionAsync(string sessionId);
    Task<List<ChatMessage>> GetSessionMessagesAsync(string sessionId);
    Task<ChatSession> CreateSessionAsync(string title, string type);
    Task UpdateSessionTitleAsync(string sessionId, string newTitle);
    Task AddMessageAsync(string sessionId, ChatMessage message);
    Task UpdateMessageAsync(ChatMessage message);
    Task<List<ChatSession>> GetChatSessionByTypeAsync(string type);
    Task<List<ChatSessionGroup>> GetChatSessionHistorysByTypeAsync(string type);

    /// <summary>
    /// 发送一条用户消息并流式接收助手回复（含推理），返回本轮的用户与助手消息对象。
    /// history 由服务内部构造（排除本轮 user 与空 assistant）。
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <param name="userContent">用户输入</param>
    /// <param name="onDelta">流式回调 (ResponseType, 分片内容)</param>
    /// <param name="onMessagesCreated">在真正开始流式前（已写入数据库）立即返回本轮 user/assistant 消息引用，便于 UI 先插入占位进行实时更新</param>
    /// <returns>(userMessage, assistantMessage)</returns>
    Task<(ChatMessage userMessage, ChatMessage assistantMessage)> SendMessageAsync(
        string sessionId,
        string userContent,
        Func<ResponseType, string, Task> onDelta,
        Action<ChatMessage, ChatMessage>? onMessagesCreated = null,
        CancellationToken cancellationToken = default,
        Action<Exception>? onError = null
    );
}
