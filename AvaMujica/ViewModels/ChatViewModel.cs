using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using AvaMujica.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaMujica.ViewModels;

public partial class ChatViewModel : ViewModelBase
{
    /// <summary>
    /// API 实例
    /// </summary>
    private readonly MyApi _api = new();

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
    public string ChatId { get; private set; }

    /// <summary>
    /// 聊天消息列表，用于绑定到 UI
    /// </summary>
    public ObservableCollection<ChatMessage> ChatMessageList { get; } = [];

    /// <summary>
    /// 关联的聊天会话
    /// </summary>
    public ChatSession? Session { get; private set; }

    public ChatViewModel()
    {
        ChatId = Guid.NewGuid().ToString();

        // 如果没有API实例，尝试添加一条提示信息
        if (_api == null)
        {
            ChatMessageList.Add(
                new ChatMessage
                {
                    Role = "assistant",
                    Content = "API 尚未初始化，请稍候再试...",
                    Time = DateTime.Now,
                }
            );
        }
    }

    /// <summary>
    /// 从现有的聊天会话创建视图模型
    /// </summary>
    /// <param name="session">聊天会话</param>
    public ChatViewModel(ChatSession session)
    {
        Session = session;
        ChatId = session.Id;
        ChatTitle = session.Title;
        StartTime = session.CreatedAt;

        // 加载会话中的消息
        foreach (var message in session.Messages)
        {
            ChatMessageList.Add(message);
        }
    }

    /// <summary>
    /// 输入文本改变时的回调
    /// </summary>
    /// <param name="value"></param>
    partial void OnInputTextChanged(string value)
    {
        SendCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// 发送消息命令
    /// </summary>
    /// <returns></returns>
    [RelayCommand(CanExecute = nameof(CanSend))]
    private async Task SendAsync()
    {
        if (string.IsNullOrEmpty(InputText))
            return;

        // 添加用户消息
        var userMessage = new ChatMessage
        {
            Role = "user",
            Content = InputText,
            Time = DateTime.Now,
        };
        ChatMessageList.Add(userMessage);

        // 通知视图滚动到底部
        NotifyScrollToBottom();

        var userInput = InputText;
        InputText = string.Empty;

        // 如果是首次对话，更新标题(使用用户的前几个字作为标题)
        if (ChatMessageList.Count == 1)
        {
            ChatTitle = userInput.Length > 10 ? userInput[..10] + "..." : userInput;
        }

        // 创建一个空的回复消息
        var responseMessage = new ChatMessage
        {
            Role = "assistant",
            Content = string.Empty,
            Time = DateTime.Now,
        };
        ChatMessageList.Add(responseMessage);

        if (_api == null)
        {
            responseMessage.Content = "API 尚未初始化，请稍候再试...";
            return;
        }

        await _api.ChatAsync(
            userInput,
            token =>
            {
                // 更新回复消息的内容
                responseMessage.Content += token;
                NotifyScrollToBottom();
            },
            reasoning =>
            {
                // 处理推理内容（如果需要）
                responseMessage.Content += reasoning;
                NotifyScrollToBottom();
            }
        );
    }

    /// <summary>
    /// 判断是否可以发送消息
    /// </summary>
    /// <returns></returns>
    private bool CanSend()
    {
        return !string.IsNullOrEmpty(InputText);
    }

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
