/*
 * @FilePath: ChatViewModel.cs
 * @Author: WangWindow 1598593280@qq.com
 * @Date: 2025-02-26 18:56:04
 * @LastEditors: WangWindow
 * @LastEditTime: 2025-02-28 01:32:19
 * 2025 by WangWindow, All Rights Reserved.
 * @Description:
 */
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
    private MyApi _api = null!;

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
    public ObservableCollection<ChatMessage> ChatMessages { get; } = [];

    public ChatViewModel(MyApi api)
    {
        ChatId = Guid.NewGuid().ToString();
        _api = api;

        // 如果没有API实例，尝试添加一条提示信息
        if (_api == null)
        {
            ChatMessages.Add(
                new ChatMessage
                {
                    Content = "API 尚未初始化，请稍候再试...",
                    Time = DateTime.Now,
                    IsFromUser = false,
                }
            );
        }
    }

    /// <summary>
    /// 设置API实例
    /// </summary>
    /// <param name="api"></param>
    public void SetApi(MyApi api)
    {
        _api = api;
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
        ChatMessages.Add(
            new ChatMessage
            {
                Content = InputText,
                Time = DateTime.Now,
                IsFromUser = true,
            }
        );

        // 通知视图滚动到底部
        NotifyScrollToBottom();

        var userInput = InputText;
        InputText = string.Empty;

        // 如果是首次对话，更新标题(使用用户的前几个字作为标题)
        if (ChatMessages.Count == 1)
        {
            ChatTitle = userInput.Length > 10 ? userInput[..10] + "..." : userInput;
        }

        // 创建一个空的回复消息
        var responseMessage = new ChatMessage
        {
            Content = string.Empty,
            Time = DateTime.Now,
            IsFromUser = false,
            IsLoading = true, // 设置为加载状态
        };
        ChatMessages.Add(responseMessage);

        if (_api == null)
        {
            responseMessage.Content = "API 尚未初始化，请稍候再试...";
            responseMessage.IsLoading = false;
            return;
        }

        // 调用 API 并实时更新回复内容
        StringBuilder contentBuffer = new();
        StringBuilder reasoningBuffer = new();

        await _api.ChatAsync(
            userInput,
            async token =>
            {
                // 将收到的 token 添加到缓存
                contentBuffer.Append(token);

                // 添加随机延迟，使打字机效果更自然
                Random random = new();
                int delay = random.Next(200, 300);
                await Task.Delay(delay);

                // 更新UI
                responseMessage.Content = contentBuffer.ToString();
            },
            async token =>
            {
                // 将推理内容添加到缓存
                reasoningBuffer.Append(token);

                // 推理过程使用较短的延迟
                Random random = new();
                int delay = random.Next(200, 300);
                await Task.Delay(delay);

                // 更新UI
                responseMessage.ReasoningContent = reasoningBuffer.ToString();
            }
        );

        // 响应完成后，关闭加载状态
        responseMessage.IsLoading = false;
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
