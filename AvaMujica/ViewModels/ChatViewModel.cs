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
public partial class ChatViewModel() : ViewModelBase
{
    private readonly ChatService _chatService = ChatService.Instance;

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
    /// 聊天开始时间
    /// </summary>
    [ObservableProperty]
    private string startTime = DateTime.Now.ToString("HH:mm");

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
        try
        {
            var messages = await _chatService.GetSessionMessagesAsync(sessionId);

            ChatMessageList.Clear();

            foreach (var message in messages)
            {
                ChatMessageList.Add(message);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"加载消息失败: {ex.Message}");

            ChatMessageList.Add(
                new ChatMessage
                {
                    Role = "system",
                    Content = $"加载消息失败: {ex.Message}",
                    SendTime = DateTime.Now,
                }
            );
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
    /// 发送消息命令
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSend))]
    private async Task SendAsync()
    {
        if (string.IsNullOrEmpty(InputText))
            return;

        string userInput = InputText;
        InputText = string.Empty;

        try
        {
            // 添加用户消息
            var userMessage = new ChatMessage
            {
                Role = "user",
                Content = userInput,
                SendTime = DateTime.Now,
            };

            ChatMessageList.Add(userMessage);
            NotifyScrollToBottom();

            // 如果是首次对话，更新标题
            if (ChatMessageList.Count <= 1)
            {
                ChatTitle = userInput.Length > 10 ? userInput[..10] + "..." : userInput;

                // 更新会话标题
                var session = await _chatService.GetSessionAsync(ChatId);
                if (session != null)
                {
                    session.Title = ChatTitle;
                    await _chatService.UpdateSessionAsync(session);
                }
            }

            // 创建一个空的回复消息，表示正在思考
            var responseMessage = new ChatMessage
            {
                Role = "assistant",
                Content = string.Empty,
                SendTime = DateTime.Now,
            };

            ChatMessageList.Add(responseMessage);
            NotifyScrollToBottom();

            // 发送消息并获取响应
            await _chatService.SendMessageAsync(
                ChatId,
                userInput,
                token =>
                {
                    var message = _chatService.GetLatestAssistantMessageAsync(ChatId).Result;
                    if (message != null)
                    {
                        responseMessage.Content = message.Content;
                        responseMessage.ReasoningContent = message.ReasoningContent;
                        NotifyScrollToBottom();
                    }
                },
                reasoning =>
                {
                    var message = _chatService.GetLatestAssistantMessageAsync(ChatId).Result;
                    if (message != null)
                    {
                        responseMessage.Content = message.Content;
                        responseMessage.ReasoningContent = message.ReasoningContent;
                        NotifyScrollToBottom();
                    }
                }
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"发送消息失败: {ex.Message}");

            ChatMessageList.Add(
                new ChatMessage
                {
                    Role = "system",
                    Content = $"发送消息失败: {ex.Message}",
                    SendTime = DateTime.Now,
                }
            );
            NotifyScrollToBottom();
        }
    }

    /// <summary>
    /// 判断是否可以发送消息
    /// </summary>
    private bool CanSend() => !string.IsNullOrEmpty(InputText);

    /// <summary>
    /// 滚动到底部的事件
    /// </summary>
    public event Action? ScrollToBottomRequested;

    /// <summary>
    /// 通知视图滚动到底部
    /// </summary>
    private void NotifyScrollToBottom()
    {
        ScrollToBottomRequested?.Invoke();
    }
}
