using System;
using System.Text.Json.Serialization;

namespace AvaMujica.Models;

/// <summary>
/// 用于保存 API 配置信息
/// </summary>
public class ApiConfig
{
    /// <summary>
    /// API 密钥
    /// </summary>
    [JsonPropertyName("api_key")]
    public string ApiKey { get; set; } =
        Environment.GetEnvironmentVariable("DEEPSEEK_API_KEY") ?? string.Empty;

    /// <summary>
    /// API 基础地址
    /// </summary>
    [JsonPropertyName("api_base")]
    public string ApiBase { get; set; } = "https://api.deepseek.com";

    /// <summary>
    /// 模型名称
    /// </summary>
    [JsonPropertyName("model")]
    public string Model { get; set; } = "deepseek-reasoner";

    /// <summary>
    /// 系统提示
    /// </summary>
    [JsonPropertyName("system_prompt")]
    public string SystemPrompt { get; set; } =
        "你是一名优秀的心理咨询师，具有丰富的咨询经验。你的工作是为用户提供情感支持，解决用户的疑问。";

    /// <summary>
    /// 是否显示推理过程
    /// </summary>
    [JsonPropertyName("show_reasoning")]
    public bool ShowReasoning { get; set; } = true;

    /// <summary>
    /// 温度参数
    /// </summary>
    [JsonPropertyName("temperature")]
    public float Temperature { get; set; } = 1.3f;

    /// <summary>
    /// 最大令牌数
    /// </summary>
    [JsonPropertyName("max_tokens")]
    public int MaxTokens { get; set; } = 2000;
}
