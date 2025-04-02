using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DeepSeek.Core;
using DeepSeek.Core.Models;

namespace AvaMujica.Services;

/// <summary>
/// DeepSeek API 服务
/// </summary>
/// <remarks>
/// 构造函数
/// </remarks>
public class ApiService(ConfigService configService)
{
    private readonly ConfigService _configService = configService;

    /// <summary>
    /// 调用 DeepSeek Chat 接口进行流式对话
    /// </summary>
    /// <param name="userPrompt">用户输入的提示</param>
    /// <param name="onStreamMessage">接收消息的回调</param>
    /// <returns>完整的回复内容及推理内容</returns>
    public async Task<(string content, string reasoning)> ChatAsync(
        string userPrompt,
        Action<StreamResponseType, string>? onStreamMessage = null
    )
    {
        // 加载配置
        var config = _configService.LoadFullConfig();

        // 创建 DeepSeek 客户端
        using var httpClient = new HttpClient
        {
            BaseAddress = new Uri(config.ApiBase),
            Timeout = TimeSpan.FromSeconds(300),
        };

        var client = new DeepSeekClient(httpClient, config.ApiKey);

        var request = new ChatRequest
        {
            Messages =
            [
                Message.NewSystemMessage(config.SystemPrompt),
                Message.NewUserMessage(userPrompt),
            ],
            Model = config.Model,
            Temperature = config.Temperature,
            MaxTokens = config.MaxTokens,
        };

        // 保存完整回复
        string fullContent = string.Empty;
        string fullReasoning = string.Empty;

        try
        {
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

                        // 只有在配置为显示推理时才回调
                        if (config.ShowReasoning)
                        {
                            onStreamMessage?.Invoke(StreamResponseType.Reasoning, reasoning);
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
                        onStreamMessage?.Invoke(StreamResponseType.Content, content);
                    }
                }
            }

            return (fullContent, fullReasoning);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"API调用失败: {ex.Message}");
            throw;
        }
    }
}

/// <summary>
/// 流式回复内容类型
/// </summary>
public enum StreamResponseType
{
    /// <summary>
    /// 推理内容
    /// </summary>
    Reasoning,

    /// <summary>
    /// 回复内容
    /// </summary>
    Content,
}
