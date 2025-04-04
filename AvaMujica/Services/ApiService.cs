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

    /// <summary>
    /// 调用 Chat 接口进行对话
    /// </summary>
    public async Task<(string content, string reasoning)> ChatAsync(string userPrompt)
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
        if (choices == null)
        {
            // 处理错误情况
            return (string.Empty, string.Empty);
        }

        await foreach (var choice in choices)
        {
            if (choice.Delta?.ReasoningContent != null)
            {
                string reasoning = choice.Delta.ReasoningContent;
                fullReasoning += reasoning;
            }

            if (choice.Delta?.Content != null)
            {
                string content = choice.Delta.Content;
                fullContent += content;
            }
        }

        return (fullContent, fullReasoning);
    }

    /// <summary>
    /// 调用 Chat 接口进行对话，支持流式回调
    /// </summary>
    public async Task ChatAsync(
        string userPrompt,
        Func<ResponseType, string, Task> onReceiveContent
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

        var choices = client.ChatStreamAsync(request, new CancellationToken());
        if (choices == null)
        {
            return;
        }

        await foreach (var choice in choices)
        {
            if (choice.Delta?.ReasoningContent != null)
            {
                string reasoning = choice.Delta.ReasoningContent;
                await onReceiveContent(ResponseType.ReasoningContent, reasoning);
            }

            if (choice.Delta?.Content != null)
            {
                string content = choice.Delta.Content;
                await onReceiveContent(ResponseType.Content, content);
            }
        }
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
