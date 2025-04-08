using System;
using System.Diagnostics;
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

        bool isReasoningComplete = false;
        bool isContentComplete = false;

        await foreach (var choice in choices)
        {
            // 处理推理内容
            if (choice.Delta?.ReasoningContent != null)
            {
                string reasoning = choice.Delta.ReasoningContent;
                await onReceiveContent(ResponseType.ReasoningContent, reasoning);
                Debug.Write($"{reasoning}");

                // 表明还在接收推理内容
                isReasoningComplete = false;
            }
            else if (!isReasoningComplete && choice.Delta?.Content != null)
            {
                // 如果开始接收Content但没有明确标记推理完成，说明推理已结束
                isReasoningComplete = true;
                Debug.WriteLine("\n==== ↑ Reasoning ====");
            }

            // 处理正常内容
            if (choice.Delta?.Content != null)
            {
                string content = choice.Delta.Content;
                await onReceiveContent(ResponseType.Content, content);
                Debug.Write($"{content}");

                // 表明还在接收正常内容
                isContentComplete = false;
            }

            // 检查是否是最后一个响应块
            if (choice.FinishReason != null)
            {
                if (!isContentComplete)
                {
                    Debug.WriteLine("\n==== ↑ Content ====");
                    isContentComplete = true;
                }
            }
        }

        // 确保在所有内容结束后，如果还没有打印过结束标记，则打印
        if (!isReasoningComplete)
        {
            Debug.WriteLine("\n==== ↑ Reasoning ====");
        }

        if (!isContentComplete)
        {
            Debug.WriteLine("\n==== ↑ Content ====");
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
