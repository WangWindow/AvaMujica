using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AvaMujica.Services;

public enum ResponseType
{
    Content,
    ReasoningContent
}

public interface IApiService
{
    /// <summary>
    /// 聊天（支持传入已格式化的历史上下文）
    /// </summary>
    /// <param name="userPrompt">本轮用户输入</param>
    /// <param name="onReceiveContent">流式回调(推理 / 内容)</param>
    /// <param name="historyMessages">可选：历史消息（不含本轮 userPrompt），用于发送给模型</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <param name="onError">错误回调</param>
    Task ChatAsync(
        string userPrompt,
        Func<ResponseType, string, Task> onReceiveContent,
        IReadOnlyList<(string role, string content, string? reasoningContent)>? historyMessages = null,
        CancellationToken cancellationToken = default,
        Action<Exception>? onError = null);
}
