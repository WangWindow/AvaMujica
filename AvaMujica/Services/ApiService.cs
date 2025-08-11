using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using DeepSeek.Core;
using DeepSeek.Core.Models;

namespace AvaMujica.Services;

/// <summary>
/// API 服务
/// </summary>
public class ApiService(IConfigService configService) : IApiService
{
    private readonly IConfigService _configService = configService;

    /// <summary>
    /// 调用 Chat 接口进行对话
    /// </summary>
    public async Task ChatAsync(string userPrompt, Func<ResponseType, string, Task> onReceiveContent, CancellationToken cancellationToken = default, Action<Exception>? onError = null)
    {
        try
        {
            // 加载配置
            var settings = _configService.LoadFullConfig();

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

            var choices = client.ChatStreamAsync(request, cancellationToken);
            if (choices == null)
            {
                return;
            }

            bool isReasoningComplete = false;
            bool isContentComplete = false;

            await foreach (var choice in choices.WithCancellation(cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();

                // 处理推理内容
                if (choice.Delta?.ReasoningContent != null)
                {
                    string reasoning = choice.Delta.ReasoningContent;
                    // 使用Dispatcher确保UI更新在UI线程上执行
                    await Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        await onReceiveContent(ResponseType.ReasoningContent, reasoning);
                    });
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
                    // 使用Dispatcher确保UI更新在UI线程上执行
                    await Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        await onReceiveContent(ResponseType.Content, content);
                    });
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
        catch (OperationCanceledException)
        {
            // 取消不视为错误，静默返回
        }
        catch (Exception ex)
        {
            onError?.Invoke(ex);
        }
    }
}