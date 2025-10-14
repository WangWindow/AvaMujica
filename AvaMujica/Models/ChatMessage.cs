using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaMujica.Models;

/// <summary>
/// 聊天消息类
/// </summary>
public partial class ChatMessage : ObservableObject
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
    /// <summary>
    /// 内容
    /// </summary>
    [ObservableProperty]
    private string content = string.Empty;

    /// <summary>
    /// 推理内容
    /// </summary>
    [ObservableProperty]
    private string? reasoningContent;

    /// <summary>
    /// 消息发送时间
    /// </summary>
    public DateTime SendTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 是否为用户消息
    /// </summary>
    public bool IsUser => string.Equals(Role, "user", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// 是否为助手消息
    /// </summary>
    public bool IsAssistant => string.Equals(Role, "assistant", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// 是否存在推理内容
    /// </summary>
    public bool HasReasoning => !string.IsNullOrWhiteSpace(ReasoningContent);

    /// <summary>
    /// 是否显示推理（由 ViewModel 统一控制）
    /// </summary>
    [ObservableProperty]
    private bool showReasoning;

    /// <summary>
    /// 创建一个系统角色的消息
    /// </summary>
    public static ChatMessage CreateSystemMessage(string sessionId, string? content = null) =>
        new()
        {
            SessionId = sessionId,
            Role = "system",
            Content =
                content
                ?? "你是一名优秀的心理咨询师，具有丰富的咨询经验。你的工作是为用户提供情感支持，解决用户的疑问。",
        };

    /// <summary>
    /// 创建一个用户角色的消息
    /// </summary>
    public static ChatMessage CreateUserMessage(string sessionId, string content) =>
        new()
        {
            SessionId = sessionId,
            Role = "user",
            Content = content,
        };

    /// <summary>
    /// 创建一个助手角色的消息
    /// </summary>
    public static ChatMessage CreateAssistantMessage(
        string sessionId,
        string content = "",
        string? reasoningContent = null
    ) =>
        new()
        {
            SessionId = sessionId,
            Role = "assistant",
            Content = content,
            ReasoningContent = reasoningContent,
        };
}
