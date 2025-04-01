using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DeepSeek.Core;
using DeepSeek.Core.Models;

namespace AvaMujica.Models;

/// <summary>
/// 直接调用 DeepSeek API
/// </summary>
public class MyApi
{
    /// <summary>
    /// API 配置
    /// </summary>
    private readonly ApiConfig _config = new();

    /// <summary>
    /// DeepSeek 客户端实例
    /// </summary>
    private readonly DeepSeekClient _client;

    /// <summary>
    /// 构造函数
    /// </summary>
    public MyApi()
    {
        // 创建 DeepSeek 客户端
        var httpClient = new HttpClient
        {
            // set your local api address
            BaseAddress = new Uri(_config.ApiBase),
            Timeout = TimeSpan.FromSeconds(300),
        };
        _client = new DeepSeekClient(httpClient, _config.ApiKey);
    }

    /// <summary>
    /// 调用 DeepSeek Chat 接口进行流式对话
    /// </summary>
    /// <param name="userPrompt">用户输入的提示</param>
    /// <param name="onReceiveToken">接收文本内容的回调</param>
    /// <param name="onReceiveReasoning">接收推理内容的回调</param>
    public async Task ChatAsync(
        string userPrompt,
        Action<string>? onReceiveToken,
        Action<string>? onReceiveReasoning
    )
    {
        var request = new ChatRequest
        {
            Messages =
            [
                Message.NewSystemMessage(_config.SystemPrompt),
                Message.NewUserMessage(userPrompt),
            ],
            Model = _config.Model,
        };

        var choices = _client.ChatStreamAsync(request, new CancellationToken());
        if (choices != null)
        {
            await foreach (var choice in choices)
            {
                Console.Write(choice.Delta?.ReasoningContent);
                onReceiveReasoning?.Invoke(choice.Delta.ReasoningContent);
            }

            await foreach (var choice in choices)
            {
                Console.Write(choice.Delta?.Content);
                onReceiveToken?.Invoke(choice.Delta.Content);
            }
        }
    }
}
