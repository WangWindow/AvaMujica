using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DeepSeek.Core;
using DeepSeek.Core.Models;

namespace AvaMujica.Services;

/// <summary>
/// API 服务
/// </summary>
public class ApiService()
{
    private readonly ConfigService _configService = ConfigService.Instance;

    /// <summary>
    /// 单例实例
    /// </summary>
    private static ApiService? _instance;
    private static readonly Lock _lock = new();
    public static ApiService Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new ApiService();
                }
            }
            return _instance;
        }
    }

    static ApiService()
    {
        _instance = new ApiService();
    }

    /// <summary>
    /// 调用 Chat 接口进行流式对话
    /// </summary>
    /// <param name="userPrompt">用户输入的提示</param>
    /// <param name="onStreamMessage">接收消息的回调</param>
    /// <returns>完整的回复内容及推理内容</returns>
    public async Task<(string content, string reasoning)> ChatAsync(
        string userPrompt,
        Action<ResponseType, string>? onStreamMessage = null
    )
    {
        // 加载配置
        var settings = _configService.LoadFullConfig();

        // 创建 DeepSeek 客户端
        using var httpClient = new HttpClient
        {
            BaseAddress = new Uri(settings.ApiBase),
            Timeout = TimeSpan.FromSeconds(300),
        };

        var client = new DeepSeekClient(httpClient, settings.ApiKey);

        var request = new ChatRequest
        {
            Messages =
            [
                Message.NewSystemMessage(settings.SystemPrompt),
                Message.NewUserMessage(userPrompt),
            ],
            Model = settings.Model,
            Temperature = settings.Temperature,
            MaxTokens = settings.MaxTokens,
        };

        // 保存完整回复
        string fullContent = string.Empty;
        string fullReasoning = string.Empty;

        var choices = client.ChatStreamAsync(request, new CancellationToken());
        if (choices != null)
        {
            // 首先收集推理内容
            await foreach (var choice in choices)
            {
                if (choice.Delta?.ReasoningContent != null)
                {
                    string reasoning = choice.Delta.ReasoningContent;
                    fullReasoning += reasoning;

                    if (!string.IsNullOrEmpty(reasoning))
                    {
                        onStreamMessage?.Invoke(ResponseType.ReasoningContent, reasoning);
                    }
                }
            }

            // 然后收集回复内容
            await foreach (var choice in choices)
            {
                if (choice.Delta?.Content != null)
                {
                    string content = choice.Delta.Content;
                    fullContent += content;
                    onStreamMessage?.Invoke(ResponseType.Content, content);
                }
            }
        }

        return (fullContent, fullReasoning);
    }
}

/// <summary>
/// 流式回复内容类型
/// </summary>
public enum ResponseType
{
    /// <summary>
    /// 推理内容
    /// </summary>
    ReasoningContent,

    /// <summary>
    /// 正常内容
    /// </summary>
    Content,
}
