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
    private string inputText = "Hello";

    public ObservableCollection<ChatMessage> Messages { get; } = new();

    [RelayCommand(CanExecute = nameof(CanSend))]
    private void Send()
    {
        if (string.IsNullOrWhiteSpace(InputText))
            return;

        Messages.Add(
            new ChatMessage
            {
                Content = InputText,
                Time = DateTime.Now,
                IsFromUser = true,
            }
        );

        InputText = string.Empty;
    }

    private bool CanSend()
    {
        return !string.IsNullOrWhiteSpace(InputText);
    }
}
