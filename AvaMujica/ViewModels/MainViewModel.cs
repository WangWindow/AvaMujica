using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using AvaMujica.Models;
using AvaMujica.Services;
using AvaMujica.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaMujica.ViewModels;

/// <summary>
/// 主视图模型
/// </summary>
public partial class MainViewModel : ViewModelBase
{
    private readonly HistoryService _historyService = HistoryService.Instance;

    /// <summary>
    /// SessionID 到 ChatViewModel 的映射字典，用于快速查找
    /// </summary>
    private readonly Dictionary<string, ChatViewModel> _chatViewModelMap = [];

    /// <summary>
    /// 侧边栏是否打开(默认关闭)
    /// </summary>
    [ObservableProperty]
    private bool isSiderOpen = false;

    /// <summary>
    /// 侧边栏视图模型
    /// </summary>
    [ObservableProperty]
    private SiderViewModel? siderViewModel;

    /// <summary>
    /// 当前活动对话
    /// </summary>
    [ObservableProperty]
    private ChatViewModel currentChat = null!;

    /// <summary>
    /// 设置视图模型
    /// </summary>
    [ObservableProperty]
    private SettingsViewModel? settingsViewModel;

    /// <summary>
    /// 是否显示设置视图
    /// </summary>
    [ObservableProperty]
    private bool isSettingsViewVisible = false;

    /// <summary>
    /// 当前选中的模块
    /// </summary>
    [ObservableProperty]
    private string currentModule = ChatSessionType.PsychologicalConsultation;

    /// <summary>
    /// 所有对话列表
    /// </summary>
    public ObservableCollection<ChatViewModel> Chats { get; } = [];

    /// <summary>
    /// 历史记录集合（按日期分组）
    /// </summary>
    public ObservableCollection<ChatSessionGroup> ChatSessionGroups { get; } = [];

    public MainViewModel()
    {
        // 初始化视图模型
        SiderViewModel = new SiderViewModel(this);
        SettingsViewModel = new SettingsViewModel(this);

        // 异步初始化数据
        _ = InitializeAsync();
    }

    /// <summary>
    /// 初始化ViewModel
    /// </summary>
    private async Task InitializeAsync()
    {
        await LoadChatsAsync();
        await LoadHistoryGroupsAsync();
    }

    /// <summary>
    /// 加载聊天会话
    /// </summary>
    private async Task LoadChatsAsync()
    {
        var sessions = await _historyService.GetAllSessionsAsync();

        Chats.Clear();
        _chatViewModelMap.Clear();

        foreach (var session in sessions)
        {
            var chatViewModel = new ChatViewModel()
            {
                ChatId = session.Id,
                ChatTitle = session.Title,
            };
            await chatViewModel.LoadMessagesAsync(session.Id);
            Chats.Add(chatViewModel);
            _chatViewModelMap[session.Id] = chatViewModel;
        }

        // 如果有会话，选择第一个作为当前会话
        if (Chats.Count > 0)
        {
            CurrentChat = Chats[0];
        }
        else
        {
            // 没有会话则创建新会话
            await CreateNewChatAsync();
        }
    }

    /// <summary>
    /// 加载历史记录分组
    /// </summary>
    private async Task LoadHistoryGroupsAsync()
    {
        var historyGroups = await _historyService.GetChatSessionHistorysByTypeAsync(
            ChatSessionType.PsychologicalConsultation
        );

        ChatSessionGroups.Clear();
        foreach (var group in historyGroups)
        {
            // 关联ChatViewModel到历史记录
            foreach (var item in group.Items)
            {
                // 使用映射字典快速查找ChatViewModel
                if (_chatViewModelMap.TryGetValue(item.Id, out var chatViewModel))
                {
                    // item.ChatViewModel = chatViewModel;
                }
            }

            ChatSessionGroups.Add(group);
        }
    }

    /// <summary>
    /// 切换到心理咨询模块
    /// </summary>
    [RelayCommand]
    public void SwitchToConsultationModule()
    {
        CurrentModule = ChatSessionType.PsychologicalConsultation;
    }

    /// <summary>
    /// 切换到心理评估模块
    /// </summary>
    [RelayCommand]
    public void SwitchToAssessmentModule()
    {
        CurrentModule = ChatSessionType.PsychologicalAssessment;
    }

    /// <summary>
    /// 切换到干预方案模块
    /// </summary>
    [RelayCommand]
    public void SwitchToInterventionModule()
    {
        CurrentModule = ChatSessionType.InterventionPlan;
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
    public async Task CreateNewChatAsync()
    {
        // 确定会话类型
        // 根据当前选中的模块确定会话类型
        string sessionType = CurrentModule;

        // 创建会话
        var session = await _historyService.CreateSessionAsync(
            $"新会话 {Chats.Count + 1}",
            sessionType
        );

        // 创建视图模型
        var newChat = new ChatViewModel() { ChatId = session.Id, ChatTitle = session.Title };

        Chats.Add(newChat);
        _chatViewModelMap[session.Id] = newChat;
        CurrentChat = newChat;

        // 刷新历史记录
        await LoadHistoryGroupsAsync();

        // 关闭侧边栏
        IsSiderOpen = false;
    }

    /// <summary>
    /// 显示设置视图
    /// </summary>
    [RelayCommand]
    public void ShowSettings()
    {
        // 初始化设置视图模型
        SettingsViewModel?.Initialize();
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

    /// <summary>
    /// 根据ID切换当前会话
    /// </summary>
    public void SwitchToChat(string sessionId)
    {
        // 使用映射字典快速查找ChatViewModel
        if (_chatViewModelMap.TryGetValue(sessionId, out var chat))
        {
            CurrentChat = chat;
            IsSiderOpen = false;
        }
    }

    /// <summary>
    /// 根据会话ID获取对应的ChatViewModel
    /// </summary>
    public ChatViewModel? GetChatViewModel(string sessionId)
    {
        _chatViewModelMap.TryGetValue(sessionId, out var chatViewModel);
        return chatViewModel;
    }

    /// <summary>
    /// 显示输入对话框
    /// </summary>
    /// <param name="title">标题</param>
    /// <param name="message">消息内容</param>
    /// <param name="inputControl">输入控件</param>
    /// <returns>如果用户点击确认返回true，否则返回false</returns>
    public async Task<bool> ShowInputDialog(string title, string message, Control inputControl)
    {
        var okButton = new Button
        {
            Content = "确认",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
        };
        var cancelButton = new Button
        {
            Content = "取消",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
        };

        var buttonPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Spacing = 10,
            Children = { cancelButton, okButton },
        };

        var dialog = new Window
        {
            Width = 400,
            Height = 200,
            Title = title,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Content = new StackPanel
            {
                Margin = new Avalonia.Thickness(20),
                Spacing = 20,
                Children =
                {
                    new TextBlock { Text = message },
                    inputControl,
                    buttonPanel,
                },
            },
        };

        var tcs = new TaskCompletionSource<bool>();

        okButton.Click += (s, e) =>
        {
            tcs.TrySetResult(true);
            dialog.Close();
        };

        cancelButton.Click += (s, e) =>
        {
            tcs.TrySetResult(false);
            dialog.Close();
        };

        dialog.Closed += (s, e) =>
        {
            if (!tcs.Task.IsCompleted)
                tcs.TrySetResult(false);
        };

        await dialog.ShowDialog(App.MainWindow);
        return await tcs.Task;
    }
}
