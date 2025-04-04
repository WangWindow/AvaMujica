using System;
using System.Collections.Generic;
using AvaMujica.ViewModels;

namespace AvaMujica.Models;

/// <summary>
/// 会话类型
/// </summary>
public static class ChatSessionType
{
    public const string PsychologicalConsultation = "心理咨询";
    public const string PsychologicalAssessment = "心理评估";
    public const string InterventionPlan = "干预方案";
}

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
    /// 关联的ChatViewModel（仅用于UI显示，不保存到数据库）
    /// </summary>
    public ChatViewModel? ChatViewModel { get; set; }

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

/// <summary>
/// 会话分组类
/// </summary>
public class ChatSessionGroup
{
    /// <summary>
    /// 分组的键
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// 分组的历史记录信息
    /// </summary>
    public List<ChatSession> Items { get; }

    public ChatSessionGroup(string key, List<ChatSession> items)
    {
        Key = key;
        Items = items;
    }
}
