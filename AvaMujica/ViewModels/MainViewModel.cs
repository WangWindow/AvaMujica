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
    /// <summary>
    /// 窗口标题
    /// </summary>
    public string Title { get; set; } = "AvaMujica";

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
    /// 侧边栏是否打开(默认关闭)
    /// </summary>

    [ObservableProperty]
    private bool isSiderOpen = false;

    /// <summary>
    /// 侧边栏视图模型
    /// </summary>
    [ObservableProperty]
    private SiderViewModel siderViewModel;

    public MainViewModel()
    {
        SiderViewModel = new SiderViewModel();
        // 初始化 API
        InitializeApi();
    }

    /// <summary>
    /// 初始化 API
    /// </summary>
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
            ChatMessages.Clear();
            ChatMessages.Add(
                new ChatMessage
                {
                    Content = $"API 初始化失败: {ex.Message}",
                    Time = DateTime.Now,
                    IsFromUser = false,
                }
            );
        }
    }

    /// <summary>
    /// 切换侧边栏
    /// </summary>
    [RelayCommand]
    private void ToggleSider()
    {
        IsSiderOpen = !IsSiderOpen;
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
    /// 聊天消息列表，用于绑定到 UI
    /// </summary>
    public ObservableCollection<ChatMessage> ChatMessages { get; } = [];

    /// <summary>
    /// 发送消息命令
    /// </summary>
    /// <returns></returns>
    [RelayCommand(CanExecute = nameof(CanSend))]
    private async Task SendAsync()
    {
        if (string.IsNullOrWhiteSpace(InputText))
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
            ChatMessages.Add(responseMessage);

            // 调用 API 并实时更新回复内容
            Console.WriteLine($"用户输入: {userInput}");
            await _api.ChatAsync(
                userInput,
                token =>
                {
                    responseMessage.Content += token;
                    Console.WriteLine(token);
                }
            );
            Console.WriteLine("Ok");
        }
        catch (Exception ex)
        {
            ChatMessages.Add(
                new ChatMessage
                {
                    Content = $"API 调用失败: {ex.Message}",
                    Time = DateTime.Now,
                    IsFromUser = false,
                }
            );
        }
    }

    /// <summary>
    /// 判断是否可以发送消息
    /// </summary>
    /// <returns></returns>
    private bool CanSend()
    {
        return !string.IsNullOrWhiteSpace(InputText);
    }
}
