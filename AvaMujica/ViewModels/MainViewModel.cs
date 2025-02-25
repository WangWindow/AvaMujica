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
    private MyApi _api = null!;
    private string _currentResponse = string.Empty;

    [ObservableProperty]
    private string inputText = string.Empty;

    [ObservableProperty]
    private bool isSiderOpen = false; // 侧边栏默认关闭

    [ObservableProperty]
    private SiderViewModel siderViewModel;

    public MainViewModel()
    {
        SiderViewModel = new SiderViewModel();
        // 初始化 API
        InitializeApi();
    }

    private async void InitializeApi()
    {
        try
        {
            var config = await MyApi.LoadConfigAsync();
            _api = new MyApi(config);
        }
        catch (Exception ex)
        {
            // 处理初始化失败
            Messages.Clear();
            Messages.Add(
                new ChatMessage
                {
                    Content = $"API 初始化失败: {ex.Message}",
                    Time = DateTime.Now,
                    IsFromUser = false,
                }
            );
        }
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

        try
        {
            // 创建一个空的回复消息
            var responseMessage = new ChatMessage
            {
                Content = string.Empty,
                Time = DateTime.Now,
                IsFromUser = false,
            };
            Messages.Add(responseMessage);

            // 调用 API 并实时更新回复内容
            await _api.ChatAsync(
                userInput,
                token =>
                {
                    responseMessage.Content += token;
                    OnPropertyChanged(nameof(Messages)); // 通知 UI 更新
                }
            );
        }
        catch (Exception ex)
        {
            Messages.Add(
                new ChatMessage
                {
                    Content = $"API 调用失败: {ex.Message}",
                    Time = DateTime.Now,
                    IsFromUser = false,
                }
            );
        }
    }

    private bool CanSend()
    {
        return !string.IsNullOrWhiteSpace(InputText);
    }
}
