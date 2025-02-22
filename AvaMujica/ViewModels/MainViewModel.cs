/*
 * @FilePath: MainViewModel.cs
 * @Author: WangWindow 1598593280@qq.com
 * @Date: 2025-02-21 16:27:39
 * @LastEditors: WangWindow
 * @LastEditTime: 2025-02-22 16:26:13
 * 2025 by WangWindow, All Rights Reserved.
 * @Description:
 */
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AvaMujica.Models;

namespace AvaMujica.ViewModels;

public class MainViewModel : ViewModelBase
{
    public string Greeting { get; } = "Welcome to AvaMujica!";
    public string Title { get; set; } = "AvaMujica";

    private string _inputText = string.Empty;
    public string InputText
    {
        get => _inputText;
        set => SetProperty(ref _inputText, value);
    }

    public ObservableCollection<ChatMessage> Messages { get; } = new();

    // 构造函数中添加一些示例消息
    public MainViewModel()
    {
        Messages.Add(
            new ChatMessage
            {
                Content = "你好！我是机器人。",
                IsFromUser = false,
                Time = DateTime.Now,
            }
        );

        Messages.Add(
            new ChatMessage
            {
                Content = "你好！",
                IsFromUser = true,
                Time = DateTime.Now,
            }
        );
    }

    // 可以添加发送消息的命令
    public void SendMessage()
    {
        if (string.IsNullOrWhiteSpace(InputText))
            return;

        Messages.Add(
            new ChatMessage
            {
                Content = InputText,
                IsFromUser = true,
                Time = DateTime.Now,
            }
        );

        InputText = string.Empty;
    }
}
