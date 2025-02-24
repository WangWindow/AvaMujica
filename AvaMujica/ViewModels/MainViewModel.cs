/*
 * @FilePath: MainViewModel.cs
 * @Author: WangWindow 1598593280@qq.com
 * @Date: 2025-02-21 16:27:39
 * @LastEditors: WangWindow
 * @LastEditTime: 2025-02-23 11:14:15
 * 2025 by WangWindow, All Rights Reserved.
 * @Description:
 */
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AvaMujica.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaMujica.ViewModels;

/// <summary>
/// 主视图模型
/// </summary>
public partial class MainViewModel : ViewModelBase
{
    public string Title { get; set; } = "AvaMujica";

    [ObservableProperty]
    private string inputText = string.Empty;

    [ObservableProperty]
    private bool isSiderOpen = false; // 侧边栏默认关闭

    [ObservableProperty]
    private SiderViewModel siderViewModel;

    public MainViewModel()
    {
        SiderViewModel = new SiderViewModel();
    }

    [RelayCommand]
    private void ToggleSider()
    {
        IsSiderOpen = !IsSiderOpen;
    }

    partial void OnInputTextChanged(string value)
    {
        SendCommand.NotifyCanExecuteChanged();
    }

    public ObservableCollection<ChatMessage> Messages { get; } = new();

    [RelayCommand(CanExecute = nameof(CanSend))]
    private async Task Send()
    {
        if (string.IsNullOrWhiteSpace(InputText))
            return;

        // 添加用户消息
        Messages.Add(
            new ChatMessage
            {
                Content = InputText,
                Time = DateTime.Now,
                IsFromUser = true,
            }
        );

        var userInput = InputText;
        InputText = string.Empty;

        // 模拟机器人回复
        await Task.Delay(1000); // 模拟网络延迟
        Messages.Add(
            new ChatMessage
            {
                Content = $"您发送的消息是: {userInput}",
                Time = DateTime.Now,
                IsFromUser = false,
            }
        );
    }

    private bool CanSend()
    {
        return !string.IsNullOrWhiteSpace(InputText);
    }
}
