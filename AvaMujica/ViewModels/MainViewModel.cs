using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AvaMujica.Models;
using AvaMujica.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace AvaMujica.ViewModels;

/// <summary>
/// 主视图模型
/// </summary>
public partial class MainViewModel : ViewModelBase
{
    private readonly IHistoryService _historyService;

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
    private string currentModule = SessionType.PsychologicalConsultation;

    /// <summary>
    /// 便捷布尔属性，供 XAML 直接绑定
    /// </summary>
    public bool IsConsultationSelected => CurrentModule == SessionType.PsychologicalConsultation;
    public bool IsAssessmentSelected => CurrentModule == SessionType.PsychologicalAssessment;
    public bool IsInterventionSelected => CurrentModule == SessionType.InterventionPlan;

    partial void OnCurrentModuleChanged(string value)
    {
        OnPropertyChanged(nameof(IsConsultationSelected));
        OnPropertyChanged(nameof(IsAssessmentSelected));
        OnPropertyChanged(nameof(IsInterventionSelected));
    }

    /// <summary>
    /// 所有对话列表
    /// </summary>
    public ObservableCollection<ChatViewModel> Chats { get; } = [];

    /// <summary>
    /// 历史记录集合（按日期分组）
    /// </summary>
    public ObservableCollection<ChatSessionGroup> ChatSessionGroups { get; } = [];

    public MainViewModel(IHistoryService historyService)
    {
        _historyService = historyService;
        // 初始化视图模型
        SiderViewModel = new SiderViewModel(this, historyService);
    SettingsViewModel = new SettingsViewModel(
            this,
            App.Services.GetRequiredService<IConfigService>(),
            App.Services.GetRequiredService<IApiService>()
        );

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
            var chatViewModel = new ChatViewModel(_historyService, App.Services.GetRequiredService<IApiService>(), App.Services.GetRequiredService<IConfigService>())
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
            SessionType.PsychologicalConsultation
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
        CurrentModule = SessionType.PsychologicalConsultation;
    }

    /// <summary>
    /// 切换到心理评估模块
    /// </summary>
    [RelayCommand]
    public void SwitchToAssessmentModule()
    {
        CurrentModule = SessionType.PsychologicalAssessment;
    }

    /// <summary>
    /// 切换到干预方案模块
    /// </summary>
    [RelayCommand]
    public void SwitchToInterventionModule()
    {
        CurrentModule = SessionType.InterventionPlan;
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
}
