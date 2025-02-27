/*
 * @FilePath: MainViewModel.cs
 * @Author: WangWindow 1598593280@qq.com
 * @Date: 2025-02-21 16:27:39
 * @LastEditors: WangWindow
 * @LastEditTime: 2025-02-28 00:17:54
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
    /// 侧边栏是否打开(默认关闭)
    /// </summary>
    [ObservableProperty]
    private bool isSiderOpen = false;

    /// <summary>
    /// 侧边栏视图模型
    /// </summary>
    [ObservableProperty]
    private SiderViewModel siderViewModel;

    /// <summary>
    /// 当前活动对话
    /// </summary>
    [ObservableProperty]
    private ChatViewModel currentChat = null!;

    /// <summary>
    /// 设置视图模型
    /// </summary>
    [ObservableProperty]
    private SettingsViewModel settingsViewModel;

    /// <summary>
    /// 是否显示设置视图
    /// </summary>
    [ObservableProperty]
    private bool isSettingsViewVisible = false;

    /// <summary>
    /// 所有对话列表
    /// </summary>
    public ObservableCollection<ChatViewModel> Chats { get; } = [];

    public MainViewModel()
    {
        SiderViewModel = new SiderViewModel(this);
        SettingsViewModel = new SettingsViewModel(this);

        InitializeApi(); // 初始化 API
        CreateNewChat(); // 创建一个初始对话
    }

    /// <summary>
    /// 初始化 API
    /// </summary>
    private void InitializeApi()
    {
        var config = MyApi.LoadConfig();
        _api = new MyApi(config);

        // 为当前对话设置API
        if (CurrentChat != null)
        {
            CurrentChat.SetApi(_api);
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
    /// 创建新对话
    /// </summary>
    [RelayCommand]
    private void CreateNewChat()
    {
        var newChat = new ChatViewModel(_api)
        {
            ChatTitle = $"新对话 {Chats.Count + 1}",
            StartTime = DateTime.Now,
        };

        Chats.Add(newChat);
        CurrentChat = newChat;

        // 将新对话添加到侧边栏历史记录
        var historyInfo = new HistoryInfo
        {
            Id = Guid.NewGuid().ToString(),
            Title = newChat.ChatTitle,
            Time = DateTime.Now,
            ChatViewModel = newChat,
        };

        SiderViewModel.AddHistoryItem(historyInfo);

        // 关闭侧边栏，让用户可以立即开始新对话
        IsSiderOpen = false;
    }

    /// <summary>
    /// 切换到指定对话
    /// </summary>
    /// <param name="chatViewModel"></param>
    [RelayCommand]
    public void SwitchChat(ChatViewModel chatViewModel)
    {
        if (chatViewModel != null && Chats.Contains(chatViewModel))
        {
            CurrentChat = chatViewModel;
        }
    }

    /// <summary>
    /// 显示设置视图
    /// </summary>
    [RelayCommand]
    public void ShowSettings()
    {
        IsSettingsViewVisible = true;
        // 显示设置视图时关闭侧边栏
        IsSiderOpen = false;
    }

    /// <summary>
    /// 返回聊天视图
    /// </summary>
    [RelayCommand]
    public void ReturnToChat()
    {
        IsSettingsViewVisible = false;
    }
}
