using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;

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
        // 将耗时的网络流式请求放到后台线程，避免 Android 抛出 NetworkOnMainThreadException
        try
        {
            await Task.Run(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                // 加载配置（本地 DB 读取，快速）
                var settings = _configService.LoadFullConfig();

                // 构建 OpenAI 兼容的 endpoint
                var baseUrl = (settings.ApiBase ?? string.Empty).TrimEnd('/');
                string endpoint;
                if (baseUrl.Contains("/chat/completions", StringComparison.OrdinalIgnoreCase))
                {
                    // 用户已提供完整 path
                    endpoint = baseUrl;
                }
                else
                {
                    // 默认 OpenAI v1
                    if (baseUrl.EndsWith("/v1", StringComparison.OrdinalIgnoreCase))
                        endpoint = baseUrl + "/chat/completions";
                    else if (baseUrl.EndsWith("/v1/", StringComparison.OrdinalIgnoreCase))
                        endpoint = baseUrl + "chat/completions";
                    else
                        endpoint = baseUrl + "/v1/chat/completions";
                }

                using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(300) };
                using var req = new HttpRequestMessage(HttpMethod.Post, endpoint);
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ApiKey);
                req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

                // 组装请求体
                var json = BuildOpenAIChatRequest(settings.Model, settings.SystemPrompt, userPrompt, settings.Temperature, settings.MaxTokens);
                req.Content = new StringContent(json, Encoding.UTF8, "application/json");

                // 发送并以流式读取
                using var response = await httpClient.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                    throw new HttpRequestException($"OpenAI API error {(int)response.StatusCode}: {err}");
                }

                await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                using var reader = new StreamReader(stream, Encoding.UTF8);

                bool isReasoningComplete = false;
                bool isContentComplete = false;

                while (!reader.EndOfStream)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue; // SSE 事件间的空行
                    }

                    // 忽略注释/keep-alive
                    if (line.StartsWith(':'))
                        continue;

                    if (!line.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                        continue;

                    var payload = line[5..].Trim(); // after 'data:'
                    if (payload == "[DONE]")
                        break;

                    try
                    {
                        using var doc = JsonDocument.Parse(payload);
                        if (!doc.RootElement.TryGetProperty("choices", out var choicesEl) || choicesEl.ValueKind != JsonValueKind.Array)
                            continue;

                        foreach (var choiceEl in choicesEl.EnumerateArray())
                        {
                            // finish_reason
                            string? finishReason = null;
                            if (choiceEl.TryGetProperty("finish_reason", out var frEl) && frEl.ValueKind != JsonValueKind.Null)
                            {
                                finishReason = frEl.GetString();
                            }

                            // delta
                            if (choiceEl.TryGetProperty("delta", out var deltaEl) && deltaEl.ValueKind == JsonValueKind.Object)
                            {
                                // reasoning_content (部分提供商/模型支持)
                                if (deltaEl.TryGetProperty("reasoning_content", out var rcEl) && rcEl.ValueKind == JsonValueKind.String)
                                {
                                    var reasoning = rcEl.GetString() ?? string.Empty;
                                    await Dispatcher.UIThread.InvokeAsync(async () =>
                                    {
                                        await onReceiveContent(ResponseType.ReasoningContent, reasoning).ConfigureAwait(false);
                                    });
                                    Debug.Write(reasoning);
                                    isReasoningComplete = false;
                                }

                                // content
                                if (deltaEl.TryGetProperty("content", out var cEl) && cEl.ValueKind == JsonValueKind.String)
                                {
                                    var content = cEl.GetString() ?? string.Empty;
                                    // 如果开始接收content且此前未明确结束推理，则标记推理完成
                                    if (!isReasoningComplete)
                                    {
                                        isReasoningComplete = true;
                                        Debug.WriteLine("\n==== ↑ Reasoning ====");
                                    }

                                    await Dispatcher.UIThread.InvokeAsync(async () =>
                                    {
                                        await onReceiveContent(ResponseType.Content, content).ConfigureAwait(false);
                                    });
                                    Debug.Write(content);
                                    isContentComplete = false;
                                }
                            }

                            // 处理结束
                            if (!string.IsNullOrEmpty(finishReason))
                            {
                                if (!isContentComplete)
                                {
                                    Debug.WriteLine("\n==== ↑ Content ====");
                                    isContentComplete = true;
                                }
                            }
                        }
                    }
                    catch (JsonException)
                    {
                        // 非 JSON 行或提供商扩展格式，忽略该行
                    }
                }

                // 结束标记确保输出完整
                if (!isReasoningComplete)
                {
                    Debug.WriteLine("\n==== ↑ Reasoning ====");
                }

                if (!isContentComplete)
                {
                    Debug.WriteLine("\n==== ↑ Content ====");
                }
            }, cancellationToken).ConfigureAwait(false);
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

    private static string BuildOpenAIChatRequest(string model, string systemPrompt, string userPrompt, double temperature, int maxTokens)
    {
        using var ms = new MemoryStream();
        using var writer = new Utf8JsonWriter(ms, new JsonWriterOptions { SkipValidation = true });
        writer.WriteStartObject();
        writer.WriteString("model", model);
        writer.WriteBoolean("stream", true);

        writer.WriteStartArray("messages");
        // system
        writer.WriteStartObject();
        writer.WriteString("role", "system");
        writer.WriteString("content", systemPrompt ?? string.Empty);
        writer.WriteEndObject();
        // user
        writer.WriteStartObject();
        writer.WriteString("role", "user");
        writer.WriteString("content", userPrompt ?? string.Empty);
        writer.WriteEndObject();
        writer.WriteEndArray();

        if (temperature > 0)
        {
            writer.WriteNumber("temperature", temperature);
        }
        if (maxTokens > 0)
        {
            writer.WriteNumber("max_tokens", maxTokens);
        }

        writer.WriteEndObject();
        writer.Flush();
        return Encoding.UTF8.GetString(ms.ToArray());
    }
}