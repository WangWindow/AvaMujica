/*
 * @FilePath: MyAPI.cs
 * @Author: WangWindow 1598593280@qq.com
 * @Date: 2025-02-21 16:27:39
 * @LastEditors: WangWindow
 * @LastEditTime: 2025-02-26 23:03:07
 * 2025 by WangWindow, All Rights Reserved.
 * @Description:
 */
using System;
using System.IO;
using System.Text.Json;
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
    /// OpenAI 聊天完成服务
    /// </summary>
    private readonly IChatCompletionService _chatCompletionService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="config"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public MyApi(ApiConfig config)
    {
        // 获取配置
        _config = config ?? throw new ArgumentNullException(nameof(config));

        // 创建 Semantic Kernel 实例
#pragma warning disable SKEXP0010 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
        _kernel = Kernel
            .CreateBuilder()
            .AddOpenAIChatCompletion(_config.Model, new Uri(_config.ApiBase), _config.ApiKey)
            .Build();
#pragma warning restore SKEXP0010 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。

        // 获取 OpenAI 聊天完成服务
        _chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
    }

    /// <summary>
    /// 异步调用 ChatGPT 接口
    /// </summary>
    /// <param name="userPrompt"></param>
    /// <param name="onReceiveToken"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task ChatAsync(string userPrompt, Action<string>? onReceiveToken = null)
    {
        try
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
                Temperature = 0.7f,
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
        catch (Exception ex)
        {
            throw new Exception($"调用 API 失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 从 api.json 加载配置
    /// </summary>
    /// <param name="configPath"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    /// <exception cref="FileNotFoundException"></exception>
    public static ApiConfig LoadConfig(string configPath = "api.json")
    {
        if (File.Exists(configPath))
        {
            try
            {
                string json = File.ReadAllText(configPath);
                ApiConfig config = JsonSerializer.Deserialize<ApiConfig>(json) ?? new ApiConfig();
                if (IsConfigValid(config))
                {
                    return config;
                }
                throw new Exception("配置项不完整");
            }
            catch (Exception ex)
            {
                throw new Exception($"加载 {configPath} 失败: {ex.Message}");
            }
        }
        throw new FileNotFoundException($"配置文件 {configPath} 不存在");
    }

    /// <summary>
    /// 检查配置是否合法
    /// </summary>
    /// <param name="config"></param>
    /// <returns></returns>
    private static bool IsConfigValid(ApiConfig config)
    {
        return !string.IsNullOrWhiteSpace(config.ApiKey)
            && !string.IsNullOrWhiteSpace(config.ApiBase)
            && !string.IsNullOrWhiteSpace(config.Model);
    }
}
