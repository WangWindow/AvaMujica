using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace AvaMujica.Models;

/// <summary>
/// 聊天消息
/// </summary>
public class ChatMessage
{
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

/// <summary>
/// 聊天管理器
/// </summary>
public class ChatManager
{
    /// <summary>
    /// 当前API实例
    /// </summary>
    private readonly MyApi _api;

    /// <summary>
    /// 会话存储路径
    /// </summary>
    private readonly string _sessionsPath;

    /// <summary>
    /// 当前会话
    /// </summary>
    public ChatSession CurrentSession { get; private set; }

    /// <summary>
    /// 所有会话列表
    /// </summary>
    public List<ChatSession> Sessions { get; private set; } = [];

    /// <summary>
    /// 构造函数
    /// </summary>
    public ChatManager()
    {
        _api = new MyApi();

        // 设置会话存储路径
        _sessionsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AvaMujica",
            "sessions"
        );

        // 确保目录存在
        if (!Directory.Exists(_sessionsPath))
        {
            Directory.CreateDirectory(_sessionsPath);
        }

        // 加载会话
        LoadSessions();

        // 创建一个默认会话（如果不存在）
        if (Sessions.Count == 0)
        {
            CurrentSession = CreateNewSession("新会话");
        }
        else
        {
            // 使用最近的会话
            CurrentSession = Sessions[0];
        }
    }

    /// <summary>
    /// 加载所有会话
    /// </summary>
    private void LoadSessions()
    {
        Sessions.Clear();

        try
        {
            var sessionFiles = Directory.GetFiles(_sessionsPath, "*.json");
            foreach (var file in sessionFiles)
            {
                try
                {
                    string json = File.ReadAllText(file);
                    var session = JsonSerializer.Deserialize<ChatSession>(json);
                    if (session != null)
                    {
                        Sessions.Add(session);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"加载会话文件 {file} 失败: {ex.Message}");
                }
            }

            // 按照更新时间排序，最新的在前面
            Sessions.Sort((a, b) => b.UpdatedAt.CompareTo(a.UpdatedAt));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"加载会话列表失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 创建新会话
    /// </summary>
    /// <param name="title">会话标题</param>
    /// <returns>新创建的会话</returns>
    public ChatSession CreateNewSession(string title = "新会话")
    {
        var session = new ChatSession { Title = title };

        // 添加到列表
        Sessions.Insert(0, session);

        // 设置当前会话
        CurrentSession = session;

        // 保存会话
        SaveSession(session);

        return session;
    }

    /// <summary>
    /// 发送消息并获取响应
    /// </summary>
    /// <param name="message">用户消息</param>
    /// <param name="onReceiveToken">接收令牌回调</param>
    /// <param name="onReceiveReasoning">接收推理内容回调</param>
    /// <returns>是否发送成功</returns>
    public async Task<bool> SendMessageAsync(
        string message,
        Action<string>? onReceiveToken = null,
        Action<string>? onReceiveReasoning = null
    )
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return false;
        }

        // 添加用户消息到当前会话
        var userMessage = new ChatMessage
        {
            Role = "user",
            Content = message,
            Time = DateTime.Now,
        };
        CurrentSession.Messages.Add(userMessage);

        // 更新会话时间
        CurrentSession.UpdatedAt = DateTime.Now;

        // 保存会话
        SaveSession(CurrentSession);

        // 创建助手消息（初始为空）
        var assistantMessage = new ChatMessage
        {
            Role = "assistant",
            Content = string.Empty,
            Time = DateTime.Now,
        };
        CurrentSession.Messages.Add(assistantMessage);

        // 推理内容
        var reasoning = new System.Text.StringBuilder();

        // 调用API获取响应
        await _api.ChatAsync(
            message,
            token =>
            {
                assistantMessage.Content += token;
                onReceiveToken?.Invoke(token);
            },
            reasoningToken =>
            {
                reasoning.Append(reasoningToken);
                assistantMessage.ReasoningContent = reasoning.ToString();
                onReceiveReasoning?.Invoke(reasoningToken);
            }
        );

        // 更新会话时间
        CurrentSession.UpdatedAt = DateTime.Now;

        // 保存会话
        SaveSession(CurrentSession);

        return true;
    }

    /// <summary>
    /// 保存会话到文件
    /// </summary>
    /// <param name="session">要保存的会话</param>
    private void SaveSession(ChatSession session)
    {
        try
        {
            string filePath = Path.Combine(_sessionsPath, $"{session.Id}.json");
            string json = JsonSerializer.Serialize(
                session,
                new JsonSerializerOptions { WriteIndented = true }
            );
            File.WriteAllText(filePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"保存会话失败: {ex.Message}");
        }
    }
}
