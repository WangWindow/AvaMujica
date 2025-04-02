using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AvaMujica.Models;
using AvaMujica.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaMujica.ViewModels;

/// <summary>
/// 聊天视图模型
/// </summary>
public partial class ChatViewModel(ChatService chatService) : ViewModelBase
{
    /// <summary>
    /// 聊天服务
    /// </summary>
    private readonly ChatService _chatService = chatService;

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
    private DateTime startTime = DateTime.Now;

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
                    Time = DateTime.Now,
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
            // 如果是首次对话，更新标题
            if (ChatMessageList.Count == 0)
            {
                ChatTitle = userInput.Length > 10 ? userInput[..10] + "..." : userInput;

                // 更新会话标题
                var session = await _chatService.GetSessionAsync(ChatId);
                session.Title = ChatTitle;
                await _chatService.UpdateSessionAsync(session);
            }

            // 立即加载用户消息，以便显示在界面上
            await LoadMessagesAsync(ChatId);

            // 添加用户消息并获取响应
            await _chatService.SendMessageAsync(
                ChatId,
                userInput,
                token =>
                {
                    // 使用Task.Run避免界面阻塞
                    Task.Run(async () =>
                    {
                        try
                        {
                            // 获取最新消息
                            var assistantMessage =
                                await _chatService.GetLatestAssistantMessageAsync(ChatId);
                            if (assistantMessage != null)
                            {
                                UpdateLatestAssistantMessage(assistantMessage);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"更新消息失败: {ex.Message}");
                        }
                    });

                    // 通知滚动到底部
                    NotifyScrollToBottom();
                },
                reasoning =>
                {
                    Task.Run(async () =>
                    {
                        try
                        {
                            // 获取最新消息
                            var assistantMessage =
                                await _chatService.GetLatestAssistantMessageAsync(ChatId);
                            if (assistantMessage != null)
                            {
                                UpdateLatestAssistantMessage(assistantMessage);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"更新推理内容失败: {ex.Message}");
                        }
                    });

                    // 通知滚动到底部
                    NotifyScrollToBottom();
                }
            );

            // 最后完全重新加载消息列表，确保界面显示完整
            await LoadMessagesAsync(ChatId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"发送消息失败: {ex.Message}");
            ChatMessageList.Add(
                new ChatMessage
                {
                    Role = "system",
                    Content = $"发送消息失败: {ex.Message}",
                    Time = DateTime.Now,
                }
            );
        }
    }

    /// <summary>
    /// 更新最新的助手消息
    /// </summary>
    private void UpdateLatestAssistantMessage(ChatMessage message)
    {
        // 查找ChatMessageList中是否已有该消息
        var existingMessage = ChatMessageList.FirstOrDefault(m => m.Id == message.Id);
        if (existingMessage != null)
        {
            // 如果存在，更新内容
            existingMessage.Content = message.Content;
            existingMessage.ReasoningContent = message.ReasoningContent;
        }
        else
        {
            // 如果不存在，添加到列表
            ChatMessageList.Add(message);
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
