using System;
using System.Collections.Generic;

namespace AvaMujica.Models;

/// <summary>
/// 会话类
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
    /// 会话类型
    /// </summary>
    public string Type { get; set; } = ChatSessionType.PsychologicalConsultation;

    /// <summary>
    /// 会话创建时间
    /// </summary>
    public DateTime CreatedTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 会话更新时间
    /// </summary>
    public DateTime UpdatedTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 会话消息列表
    /// </summary>
    public List<ChatMessage> Messages { get; set; } = [];

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="title">会话标题</param>
    /// <param name="type">会话类型</param>
    public ChatSession(string? title = null, string? type = null)
    {
        if (title != null)
            Title = title;
        if (type != null)
            Type = type;
    }
}
