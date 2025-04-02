using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AvaMujica.Models;

namespace AvaMujica.Services;

/// <summary>
/// 聊天服务，提供会话管理和消息处理功能
/// </summary>
/// <remarks>
/// 构造函数
/// </remarks>
public class ChatService(
    ConfigService configService,
    ApiService apiService,
    DatabaseService databaseService,
    HistoryService historyService
)
{
    /// <summary>
    /// 配置服务
    /// </summary>
    private readonly ConfigService _configService = configService;

    /// <summary>
    /// API服务
    /// </summary>
    private readonly ApiService _apiService = apiService;

    /// <summary>
    /// 数据库服务
    /// </summary>
    private readonly DatabaseService _databaseService = databaseService;

    /// <summary>
    /// 历史记录服务
    /// </summary>
    private readonly HistoryService _historyService = historyService;

    /// <summary>
    /// 获取所有会话
    /// </summary>
    /// <returns>会话列表</returns>
    public async Task<List<ChatSession>> GetAllSessionsAsync()
    {
        return await _databaseService.GetAllSessionsAsync();
    }

    /// <summary>
    /// 获取指定会话
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <returns>会话对象</returns>
    public async Task<ChatSession> GetSessionAsync(string sessionId)
    {
        return await _databaseService.GetSessionAsync(sessionId);
    }

    /// <summary>
    /// 创建新会话
    /// </summary>
    /// <param name="title">会话标题</param>
    /// <param name="type">会话类型</param>
    /// <returns>新创建的会话</returns>
    public async Task<ChatSession> CreateSessionAsync(string title = "新会话", string type = "咨询")
    {
        return await _databaseService.CreateSessionAsync(title, type);
    }

    /// <summary>
    /// 更新会话信息
    /// </summary>
    /// <param name="session">要更新的会话</param>
    public async Task UpdateSessionAsync(ChatSession session)
    {
        await _databaseService.UpdateSessionAsync(session);
    }

    /// <summary>
    /// 删除会话
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    public async Task DeleteSessionAsync(string sessionId)
    {
        await _databaseService.DeleteSessionAsync(sessionId);
    }

    /// <summary>
    /// 添加消息到会话
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <param name="message">消息对象</param>
    public async Task<ChatMessage> AddMessageAsync(string sessionId, ChatMessage message)
    {
        message.SessionId = sessionId;
        return await _databaseService.AddMessageAsync(message);
    }

    /// <summary>
    /// 更新消息内容
    /// </summary>
    /// <param name="message">要更新的消息</param>
    public async Task UpdateMessageAsync(ChatMessage message)
    {
        await _databaseService.UpdateMessageAsync(message);
    }

    /// <summary>
    /// 获取指定会话的所有消息
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <returns>消息列表</returns>
    public async Task<List<ChatMessage>> GetSessionMessagesAsync(string sessionId)
    {
        return await _databaseService.GetSessionMessagesAsync(sessionId);
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
        var userMessage = new ChatMessage
        {
            SessionId = sessionId,
            Role = "user",
            Content = userContent,
            Time = DateTime.Now,
        };

        await AddMessageAsync(sessionId, userMessage);

        // 创建助手消息（初始为空）
        var assistantMessage = new ChatMessage
        {
            SessionId = sessionId,
            Role = "assistant",
            Content = string.Empty,
            ReasoningContent = string.Empty,
            Time = DateTime.Now,
        };

        // 先保存空消息到数据库
        await AddMessageAsync(sessionId, assistantMessage);

        try
        {
            // 调用API获取响应，并随时更新数据库
            await _apiService.ChatAsync(
                userContent,
                async (type, message) =>
                {
                    if (type == StreamResponseType.Content)
                    {
                        // 更新内容
                        assistantMessage.Content += message;
                        onReceiveToken?.Invoke(message);

                        // 每收到一块内容就更新数据库
                        await UpdateMessageAsync(assistantMessage);
                    }
                    else if (
                        type == StreamResponseType.Reasoning
                        && _configService.GetShowReasoning()
                    )
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
        }
        catch (Exception ex)
        {
            // 出错时也要保存消息，但添加错误信息
            assistantMessage.Content = $"发生错误: {ex.Message}";
            await UpdateMessageAsync(assistantMessage);
            throw; // 重新抛出异常，让调用者可以处理
        }

        return assistantMessage;
    }

    /// <summary>
    /// 获取指定会话的最新助手消息
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <returns>最新的助手消息，如果不存在则返回null</returns>
    public async Task<ChatMessage?> GetLatestAssistantMessageAsync(string sessionId)
    {
        return await _databaseService.GetLatestAssistantMessageAsync(sessionId);
    }

    /// <summary>
    /// 获取当前配置
    /// </summary>
    /// <returns>配置对象</returns>
    public Config GetCurrentConfig()
    {
        return _configService.LoadFullConfig();
    }

    /// <summary>
    /// 保存配置
    /// </summary>
    /// <param name="config">配置对象</param>
    public void SaveConfig(Config config)
    {
        _configService.SaveFullConfig(config);
    }

    /// <summary>
    /// 获取是否显示推理过程
    /// </summary>
    /// <returns>是否显示推理过程</returns>
    public bool GetShowReasoning()
    {
        return _configService.GetShowReasoning();
    }

    /// <summary>
    /// 获取会话历史记录分组
    /// </summary>
    /// <returns>按日期分组的历史记录</returns>
    public async Task<List<HistoryGroup>> GetHistoryGroupsAsync()
    {
        return await _historyService.GetHistoryGroupsAsync();
    }

    /// <summary>
    /// 按类型筛选会话历史记录分组
    /// </summary>
    /// <param name="type">会话类型</param>
    /// <returns>按日期分组的历史记录</returns>
    public async Task<List<HistoryGroup>> GetHistoryGroupsByTypeAsync(string type)
    {
        return await _historyService.GetHistoryGroupsByTypeAsync(type);
    }
}
