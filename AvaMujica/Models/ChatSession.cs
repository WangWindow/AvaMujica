using System;
using System.Collections.Generic;

namespace AvaMujica.Models;

/// <summary>
/// 聊天会话
/// </summary>
public class ChatSession
{
    /// <summary>
    /// 会话ID
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// 会话标题
    /// </summary>
    public string Title { get; set; } = "新会话";

    /// <summary>
    /// 会话类型 (咨询、评估、干预)
    /// </summary>
    public string Type { get; set; } = "咨询";

    /// <summary>
    /// 会话创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// 会话更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// 会话消息列表
    /// </summary>
    public List<ChatMessage> Messages { get; set; } = [];
}
