using System;
using System.Collections.Generic;

namespace AvaMujica.Models;

/// <summary>
/// 配置
/// </summary>
public class Config
{
    /// <summary>
    /// 语言
    /// </summary>
    public string Language { get; set; } = "zh-CN";

    /// <summary>
    /// 主题
    /// </summary>
    public string Theme { get; set; } = "Auto";

    /// <summary>
    /// 系统提示词
    /// </summary>
    public string SystemPrompt { get; set; } =
        "你是Ava，一位专业心理咨询师，拥有丰富的心理学知识和咨询经验。你温暖、专业、善解人意，能够帮助来访者探索情感、认知和行为模式，提供有效的心理支持。";

    /// <summary>
    /// API Key
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// API基础URL
    /// </summary>
    public string ApiBase { get; set; } = "https://api.deepseek.com";

    /// <summary>
    /// API模型名称
    /// </summary>
    public string Model { get; set; } = ChatModels.Chat;

    /// <summary>
    /// 温度参数
    /// </summary>
    public float Temperature { get; set; } = 1.2f;

    /// <summary>
    /// 最大生成token数
    /// </summary>
    public int MaxTokens { get; set; } = 2000;

    /// <summary>
    /// 是否显示 Reasoning
    /// </summary>
    public bool IsShowReasoning { get; set; } = true;
}

/// <summary>
/// 配置适配器，用于数据库操作
/// </summary>
public class ConfigAdapter
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
