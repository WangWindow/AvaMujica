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
        "你是一名心理学专家。请回答以下心理学案例题目，请逐步思考，仔细分析给定的心理学案例，首先给出你的推理过程，以及得出该推理结论的详细解释和事实理由，解释你是从什么事实中得出结论的，然后给出答案。注意，你必须在<think></think>标签内给出你的推理过程，然后，在</think>标签后给出最终的答案。";

    /// <summary>
    /// API Key
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// API基础URL
    /// </summary>
    public string ApiBase { get; set; } = "https://api.openai.com";

    /// <summary>
    /// API模型名称
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// 温度参数
    /// </summary>
    public float Temperature { get; set; } = 0.8f;

    /// <summary>
    /// 最大生成token数
    /// </summary>
    public int MaxTokens { get; set; } = 2048;

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
