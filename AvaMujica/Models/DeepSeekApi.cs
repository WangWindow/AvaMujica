/*
==============================================================================
 * @FilePath: DeepSeekApi.cs
 * @Author: WangWindow 1598593280@qq.com
 * @Date: 2025-02-28 10:26:34
 * @LastEditors: WangWindow 1598593280@qq.com
 * @LastEditTime: 2025-03-18 22:13:21
 * @Copyright © 2025 WangWindow
 * @Descripttion:
==============================================================================
*/
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AvaMujica.Models;

/// <summary>
/// 用于支持DeepSeek Reasoner特有的推理内容
/// </summary>
public class DeepSeekMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}

/// <summary>
/// DeepSeek API请求体
/// </summary>
public class DeepSeekRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("messages")]
    public List<DeepSeekMessage> Messages { get; set; } = new List<DeepSeekMessage>();

    [JsonPropertyName("temperature")]
    public float Temperature { get; set; }

    [JsonPropertyName("max_tokens")]
    public int MaxTokens { get; set; }

    [JsonPropertyName("stream")]
    public bool Stream { get; set; }
}

/// <summary>
/// DeepSeek推理内容响应
/// </summary>
public class DeepSeekChoice
{
    [JsonPropertyName("message")]
    public DeepSeekResponseMessage Message { get; set; } = new DeepSeekResponseMessage();
}

/// <summary>
/// DeepSeek API响应消息
/// </summary>
public class DeepSeekResponseMessage
{
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("reasoning_content")]
    public string ReasoningContent { get; set; } = string.Empty;

    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;
}

/// <summary>
/// DeepSeek API响应
/// </summary>
public class DeepSeekResponse
{
    [JsonPropertyName("choices")]
    public List<DeepSeekChoice> Choices { get; set; } = new List<DeepSeekChoice>();
}

/// <summary>
/// DeepSeek 流式响应格式
/// </summary>
public class DeepSeekStreamResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("object")]
    public string Object { get; set; } = string.Empty;

    [JsonPropertyName("created")]
    public long Created { get; set; }

    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("choices")]
    public List<DeepSeekStreamChoice> Choices { get; set; } = new();
}

/// <summary>
/// DeepSeek 流式响应选择项
/// </summary>
public class DeepSeekStreamChoice
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("delta")]
    public DeepSeekDelta Delta { get; set; } = new();

    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; set; }
}

/// <summary>
/// DeepSeek 流式响应内容增量
/// </summary>
public class DeepSeekDelta
{
    [JsonPropertyName("role")]
    public string? Role { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("reasoning_content")]
    public string? ReasoningContent { get; set; }
}
