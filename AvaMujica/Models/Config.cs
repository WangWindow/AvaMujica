using System;

namespace AvaMujica.Models;

/// <summary>
/// 配置模型
/// </summary>
public class Config
{
    /// <summary>
    /// 配置键
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// 配置值
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// 配置ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// API 密钥
    /// </summary>
    public string ApiKey { get; set; } =
        Environment.GetEnvironmentVariable("DEEPSEEK_API_KEY") ?? string.Empty;

    /// <summary>
    /// API 基础地址
    /// </summary>
    public string ApiBase { get; set; } = "https://api.deepseek.com";

    /// <summary>
    /// 模型名称
    /// </summary>
    public string Model { get; set; } = "deepseek-reasoner";

    /// <summary>
    /// 系统提示
    /// </summary>
    public string SystemPrompt { get; set; } =
        "你是一名优秀的心理咨询师，具有丰富的咨询经验。你的工作是为用户提供情感支持，解决用户的疑问。";

    /// <summary>
    /// 是否显示推理过程
    /// </summary>
    public bool ShowReasoning { get; set; } = true;

    /// <summary>
    /// 温度参数
    /// </summary>
    public float Temperature { get; set; } = 1.3f;

    /// <summary>
    /// 最大令牌数
    /// </summary>
    public int MaxTokens { get; set; } = 2000;
}
