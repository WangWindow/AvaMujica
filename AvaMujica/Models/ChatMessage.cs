using System;

namespace AvaMujica.Models;

/// <summary>
/// 聊天消息类
/// </summary>
public class ChatMessage
{
    /// <summary>
    /// 消息ID
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// 会话ID
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// 角色
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// 内容
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 推理内容
    /// </summary>
    public string? ReasoningContent { get; set; }

    /// <summary>
    /// 消息发送时间
    /// </summary>
    public DateTime SendTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 构造函数
    /// </summary>
    public ChatMessage() { }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <param name="role">角色</param>
    /// <param name="content">内容</param>
    /// <param name="reasoningContent">推理内容</param>
    public ChatMessage(
        string sessionId,
        string role,
        string content,
        string? reasoningContent = null
    )
    {
        SessionId = sessionId;
        Role = role;
        Content = content;
        ReasoningContent = reasoningContent;
    }

    /// <summary>
    /// 创建一个系统角色的消息
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <param name="content">内容</param>
    /// <returns>系统消息</returns>
    public static ChatMessage CreateSystemMessage(string sessionId, string? content = null)
    {
        return new ChatMessage(
            sessionId,
            "system",
            content
                ?? "你是一名优秀的心理咨询师，具有丰富的咨询经验。你的工作是为用户提供情感支持，解决用户的疑问。"
        );
    }

    /// <summary>
    /// 创建一个用户角色的消息
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <param name="content">内容</param>
    /// <returns>用户消息</returns>
    public static ChatMessage CreateUserMessage(string sessionId, string content)
    {
        return new ChatMessage(sessionId, "user", content);
    }

    /// <summary>
    /// 创建一个助手角色的消息
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <param name="content">内容</param>
    /// <param name="reasoningContent">推理内容</param>
    /// <returns>助手消息</returns>
    public static ChatMessage CreateAssistantMessage(
        string sessionId,
        string content = "",
        string? reasoningContent = null
    )
    {
        return new ChatMessage(sessionId, "assistant", content, reasoningContent);
    }
}
