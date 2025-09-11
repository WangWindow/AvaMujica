using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using AvaMujica.Models;
using AvaMujica.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace AvaMujica.ViewModels;

public partial class ChatViewModel : ViewModelBase
{
    private readonly IApiService _apiService;
    private readonly IHistoryService _historyService;
    private readonly IConfigService _configService;
    private readonly CancellationTokenSource? _cts = null;

    [ObservableProperty]
    private string inputText = string.Empty;

    [ObservableProperty]
    private string chatTitle = "新对话";

    [ObservableProperty]
    private bool isShowReasoning = true;

    public bool ShouldShowReasoning(ChatMessage m) => IsShowReasoning && m.HasReasoning;

    partial void OnChatTitleChanged(string value)
    {
        if (!string.IsNullOrEmpty(ChatId) && !string.IsNullOrEmpty(value))
            _ = UpdateChatTitleAsync(ChatId, value);
    }

    private async Task UpdateChatTitleAsync(string sessionId, string newTitle)
    {
        var session = await _historyService.GetSessionAsync(sessionId);
        if (session != null)
        {
            session.Title = newTitle;
            await _historyService.UpdateSessionTitleAsync(sessionId, newTitle);
        }
    }

    public ChatViewModel() : this(
        App.Services.GetRequiredService<IHistoryService>(),
        App.Services.GetRequiredService<IApiService>(),
        App.Services.GetRequiredService<IConfigService>())
    { }

    public ChatViewModel(IHistoryService historyService, IApiService apiService, IConfigService configService)
    {
        _historyService = historyService;
        _apiService = apiService;
        _configService = configService;
        LoadConfiguration();
    }

    public void LoadConfiguration()
    {
        var config = _configService.LoadFullConfig();
        IsShowReasoning = config.IsShowReasoning;
        foreach (var m in ChatMessageList)
        {
            m.ShowReasoning = ShouldShowReasoning(m);
        }
    }

    public string ChatId { get; set; } = Guid.NewGuid().ToString();

    public ObservableCollection<ChatMessage> ChatMessageList { get; } = [];

    public async Task LoadMessagesAsync(string sessionId)
    {
        var messages = await _historyService.GetSessionMessagesAsync(sessionId);
        ChatMessageList.Clear();
        foreach (var message in messages)
        {
            message.ShowReasoning = ShouldShowReasoning(message);
            ChatMessageList.Add(message);
        }
    }

    partial void OnInputTextChanged(string value) => SendCommand.NotifyCanExecuteChanged();

    [RelayCommand]
    private async Task QuickMessage(string message)
    {
        string enhancedMessage = message switch
        {
            "感到焦虑" => "我最近感到非常焦虑，总是担心各种事情，请给我一些建议。",
            "需要放松" => "我最近压力很大，需要一些放松的方法，能介绍几种简单有效的放松技巧吗？",
            "睡眠问题" => "我最近睡眠质量很差，难以入睡，请问有什么改善睡眠的方法？",
            _ => message
        };
        InputText = enhancedMessage;
        await SendAsync();
    }

    [RelayCommand(CanExecute = nameof(CanSend))]
    private async Task SendAsync()
    {
        if (string.IsNullOrEmpty(InputText)) return;

        string userInput = InputText;
        InputText = string.Empty;

        var userMessage = ChatMessage.CreateUserMessage(ChatId, userInput);
        userMessage.ShowReasoning = ShouldShowReasoning(userMessage);
        ChatMessageList.Add(userMessage);
        await _historyService.AddMessageAsync(ChatId, userMessage);

        var assistantMessage = ChatMessage.CreateAssistantMessage(ChatId, string.Empty, string.Empty);
        assistantMessage.ShowReasoning = ShouldShowReasoning(assistantMessage);
        ChatMessageList.Add(assistantMessage);
        await _historyService.AddMessageAsync(ChatId, assistantMessage);

        // 构造历史上下文（排除本轮消息）
        var history = new List<(string role, string content, string? reasoningContent)>();
        foreach (var msg in ChatMessageList)
        {
            if (msg == userMessage || msg == assistantMessage) continue;
            history.Add((msg.Role, msg.Content, msg.ReasoningContent));
        }

        _ = _apiService.ChatAsync(
            userInput,
            async (type, delta) =>
            {
                if (type == ResponseType.Content)
                {
                    assistantMessage.Content += delta;
                }
                else if (type == ResponseType.ReasoningContent)
                {
                    assistantMessage.ReasoningContent += delta;
                    assistantMessage.ShowReasoning = ShouldShowReasoning(assistantMessage);
                }
                await _historyService.UpdateMessageAsync(assistantMessage);
            },
            history,
            _cts?.Token ?? CancellationToken.None,
            ex =>
            {
                assistantMessage.Content += $"\n[错误] {ex.Message}";
            }
        );

        if (ChatMessageList.Count <= 2)
        {
            ChatTitle = userInput.Length > 10 ? userInput[..10] + "..." : userInput;
            var session = await _historyService.GetSessionAsync(ChatId);
            if (session != null)
            {
                session.Title = ChatTitle;
            }
        }
    }

    private bool CanSend() => !string.IsNullOrEmpty(InputText);
}
