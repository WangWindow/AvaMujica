using System;

namespace AvaMujica.Models;

/// <summary>
/// 聊天消息
/// </summary>
public class ChatMessage
{
    /// <summary>
    /// 消息ID
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// 所属会话ID
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// 消息角色：user, assistant, system
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// 消息内容
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 消息时间
    /// </summary>
    public DateTime Time { get; set; } = DateTime.Now;

    /// <summary>
    /// 推理内容（仅适用于assistant角色）
    /// </summary>
    public string? ReasoningContent { get; set; }
}
