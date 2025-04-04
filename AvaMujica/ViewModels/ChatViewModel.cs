using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AvaMujica.Models;
using AvaMujica.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaMujica.ViewModels;

/// <summary>
/// 聊天视图模型
/// </summary>
public partial class ChatViewModel : ViewModelBase
{
    private readonly ApiService _apiService = ApiService.Instance;
    private readonly HistoryService _historyService = HistoryService.Instance;

    /// <summary>
    /// 输入文本
    /// </summary>
    [ObservableProperty]
    private string inputText = string.Empty;

    /// <summary>
    /// 聊天标题
    /// </summary>
    [ObservableProperty]
    private string chatTitle = "新对话";

    /// <summary>
    /// 聊天ID
    /// </summary>
    public string ChatId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// 聊天消息列表，用于绑定到 UI
    /// </summary>
    public ObservableCollection<ChatMessage> ChatMessageList { get; } = [];

    /// <summary>
    /// 加载会话消息
    /// </summary>
    public async Task LoadMessagesAsync(string sessionId)
    {
        var messages = await _historyService.GetSessionMessagesAsync(sessionId);

        ChatMessageList.Clear();

        foreach (var message in messages)
        {
            ChatMessageList.Add(message);
        }
    }

    /// <summary>
    /// 输入文本改变时的回调
    /// </summary>
    partial void OnInputTextChanged(string value)
    {
        SendCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// 快速消息命令
    /// </summary>
    [RelayCommand]
    private async Task QuickMessage(string message)
    {
        // 针对特定快捷消息添加更详细的描述
        string enhancedMessage = message;
        switch (message)
        {
            case "感到焦虑":
                enhancedMessage = "我最近感到非常焦虑，总是担心各种事情，请给我一些建议。";
                break;
            case "需要放松":
                enhancedMessage =
                    "我最近压力很大，需要一些放松的方法，能介绍几种简单有效的放松技巧吗？";
                break;
            case "睡眠问题":
                enhancedMessage = "我最近睡眠质量很差，难以入睡，请问有什么改善睡眠的方法？";
                break;
        }

        InputText = enhancedMessage;
        await SendAsync();
    }

    /// <summary>
    /// 发送消息命令
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSend))]
    private async Task SendAsync()
    {
        if (string.IsNullOrEmpty(InputText))
            return;

        string userInput = InputText;
        InputText = string.Empty;

        // 添加用户消息到UI
        var userMessage = new ChatMessage
        {
            Role = "user",
            Content = userInput,
            SendTime = DateTime.Now,
            SessionId = ChatId,
        };

        ChatMessageList.Add(userMessage);
        await _historyService.AddMessageAsync(ChatId, userMessage);

        // 如果是首次对话，更新标题
        if (ChatMessageList.Count <= 1)
        {
            ChatTitle = userInput.Length > 10 ? userInput[..10] + "..." : userInput;

            // 更新会话标题
            var session = await _historyService.GetSessionAsync(ChatId);
            if (session != null)
            {
                session.Title = ChatTitle;
            }
        }

        // 创建回复消息
        var responseMessage = new ChatMessage
        {
            Role = "assistant",
            Content = string.Empty,
            ReasoningContent = string.Empty,
            SendTime = DateTime.Now,
            SessionId = ChatId,
        };

        ChatMessageList.Add(responseMessage);
        await _historyService.AddMessageAsync(ChatId, responseMessage);

        // 直接使用ApiService发送消息并获取响应
        await _apiService.ChatAsync(
            userInput,
            async (type, content) =>
            {
                if (type == ResponseType.Content)
                {
                    responseMessage.Content += content;
                }
                else if (type == ResponseType.ReasoningContent)
                {
                    responseMessage.ReasoningContent += content;
                }

                // 更新数据库中的消息
                await _historyService.UpdateMessageAsync(responseMessage);
            }
        );
    }

    /// <summary>
    /// 判断是否可以发送消息
    /// </summary>
    private bool CanSend() => !string.IsNullOrEmpty(InputText);
}
