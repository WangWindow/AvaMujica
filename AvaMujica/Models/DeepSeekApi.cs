using System.Collections.Generic;
using System.Text.Json.Serialization;

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
