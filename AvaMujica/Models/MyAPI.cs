/*
 * @FilePath: MyAPI.cs
 * @Author: WangWindow 1598593280@qq.com
 * @Date: 2025-02-21 16:27:39
 * @LastEditors: WangWindow
 * @LastEditTime: 2025-02-28 17:38:30
 * 2025 by WangWindow, All Rights Reserved.
 * @Description:
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace AvaMujica.Models;

/// <summary>
/// 用于保存 API 配置信息
/// </summary>
public class ApiConfig
{
    /// <summary>
    /// API 密钥
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// API 基础地址
    /// </summary>
    public string ApiBase { get; set; } = string.Empty;

    /// <summary>
    /// 模型名称
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// 系统提示
    /// </summary>
    public string SystemPrompt { get; set; } = string.Empty;

    /// <summary>
    /// 是否显示推理过程
    /// </summary>
    public bool ShowReasoning { get; set; } = false;
}

/// <summary>
/// 使用 Semantic Kernel 调用 OpenAI API
/// </summary>
public class MyApi
{
    /// <summary>
    /// API 配置
    /// </summary>
    private readonly ApiConfig _config;

    /// <summary>
    /// Semantic Kernel 实例
    /// </summary>
    private readonly Kernel _kernel;

    /// <summary>
    /// HttpClient实例
    /// </summary>
    private readonly HttpClient _httpClient;

    /// <summary>
    /// OpenAI 聊天完成服务
    /// </summary>
    private readonly IChatCompletionService _chatCompletionService;

    /// <summary>
    /// 保存聊天历史
    /// </summary>
    private readonly List<DeepSeekMessage> _messages = new List<DeepSeekMessage>();

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="config"></param>
    public MyApi(ApiConfig config)
    {
        // 获取配置
        _config = config;

        // 创建忽略证书验证的 HttpClientHandler
        _httpClient = new HttpClient(
            new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true,
            }
        );

        // 设置基础URL
        _httpClient.BaseAddress = new Uri(_config.ApiBase);

        // 设置认证头
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config.ApiKey}");

        // 创建 Semantic Kernel 实例
#pragma warning disable SKEXP0010 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
        _kernel = Kernel
            .CreateBuilder()
            .AddOpenAIChatCompletion(
                modelId: _config.Model,
                apiKey: _config.ApiKey,
                httpClient: _httpClient,
                endpoint: new Uri(_config.ApiBase)
            )
            .Build();

        // 获取 OpenAI 聊天完成服务
        _chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();

        // 添加系统消息到历史记录
        if (!string.IsNullOrEmpty(_config.SystemPrompt))
        {
            _messages.Add(new DeepSeekMessage { Role = "system", Content = _config.SystemPrompt });
        }
    }

    /// <summary>
    /// 调用 Chat 接口
    /// </summary>
    /// <param name="userPrompt"></param>
    /// <param name="onReceiveToken"></param>
    /// <param name="onReceiveReasoning"></param>
    public async Task ChatAsync(
        string userPrompt,
        Action<string>? onReceiveToken = null,
        Action<string>? onReceiveReasoning = null
    )
    {
        // 如果不需要获取推理内容，使用标准的Semantic Kernel流程
        if (!_config.ShowReasoning)
        {
            await StandardChatAsync(userPrompt, onReceiveToken);
            return;
        }

        // 添加用户消息到历史记录
        _messages.Add(new DeepSeekMessage { Role = "user", Content = userPrompt });

        // 创建请求体
        var request = new DeepSeekRequest
        {
            Model = _config.Model,
            Messages = _messages,
            Temperature = 1.3f,
            MaxTokens = 2000,
            Stream = true, // 启用流式处理
        };

        // 构建请求内容
        var requestJson = JsonSerializer.Serialize(request);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        // 发送请求
        var response = await _httpClient.PostAsync("/chat/completions", content);

        // 确保请求成功
        response.EnsureSuccessStatusCode();

        // 获取流式响应
        using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);

        // 用于存储完整响应内容和推理内容
        var fullContent = new StringBuilder();
        var reasoningContent = new StringBuilder();

        // 逐行读取流式数据
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();

            // 跳过空行和data前缀
            if (string.IsNullOrEmpty(line) || !line.StartsWith("data: "))
                continue;

            // 去掉前缀"data: "
            var data = line.Substring(6);

            // 如果是 [DONE] 则结束处理
            if (data == "[DONE]")
                break;

            // 解析JSON响应
            var streamResponse = JsonSerializer.Deserialize<DeepSeekStreamResponse>(data);

            if (streamResponse != null && streamResponse.Choices.Count > 0)
            {
                var choice = streamResponse.Choices[0];

                // 处理token部分
                if (choice.Delta.Content != null)
                {
                    fullContent.Append(choice.Delta.Content);
                    onReceiveToken?.Invoke(choice.Delta.Content);
                }

                // 处理推理部分
                if (choice.Delta.ReasoningContent != null)
                {
                    reasoningContent.Append(choice.Delta.ReasoningContent);
                    onReceiveReasoning?.Invoke(choice.Delta.ReasoningContent);
                }
            }
        }

        // 将助手回复添加到历史记录
        _messages.Add(new DeepSeekMessage { Role = "assistant", Content = fullContent.ToString() });
    }

    /// <summary>
    /// 使用标准Semantic Kernel方式调用聊天接口
    /// </summary>
    private async Task StandardChatAsync(string userPrompt, Action<string>? onReceiveToken = null)
    {
        // 创建聊天历史
        var chatHistory = new ChatHistory();

        // 添加系统消息
        if (!string.IsNullOrEmpty(_config.SystemPrompt))
        {
            chatHistory.AddSystemMessage(_config.SystemPrompt);
        }

        // 添加用户消息
        chatHistory.AddUserMessage(userPrompt);

        // 设置流式处理选项
        var executionSettings = new OpenAIPromptExecutionSettings
        {
            Temperature = 1.3f,
            MaxTokens = 2000,
        };

        // 流式处理聊天完成
        var result = _chatCompletionService.GetStreamingChatMessageContentsAsync(
            chatHistory,
            executionSettings
        );
        await foreach (var content in result)
        {
            if (content.Content != null)
            {
                onReceiveToken?.Invoke(content.Content);
            }
        }
    }

    /// <summary>
    /// 加载配置
    /// </summary>
    public static ApiConfig LoadConfig()
    {
        return new ApiConfig
        {
            ApiKey = Environment.GetEnvironmentVariable("DEEPSEEK_KEY") ?? string.Empty,
            ApiBase = "https://api.deepseek.com/v1",
            Model = "deepseek-reasoner",
            SystemPrompt =
                "你是一名优秀的心理咨询师，具有丰富的咨询经验。你的工作是为用户提供情感支持，解决用户的疑问。",
            ShowReasoning = true,
        };
    }
}
