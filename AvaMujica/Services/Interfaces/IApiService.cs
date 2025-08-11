using System;
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
    Task ChatAsync(string userPrompt, Func<ResponseType, string, Task> onReceiveContent, CancellationToken cancellationToken = default, Action<Exception>? onError = null);
}
