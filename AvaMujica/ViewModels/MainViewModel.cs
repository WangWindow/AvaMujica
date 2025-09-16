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
    /// 当前选中的模块(路由键)，与选中标签页同步
    /// </summary>
    [ObservableProperty]
    private string currentModule = SessionType.Chat;

    /// <summary>
    /// 选中的标签页索引：0-咨询，1-评估，2-方案
    /// </summary>
    [ObservableProperty]
    private int selectedTabIndex = 0;

    // 模块 ViewModels
    [ObservableProperty]
    private AssessmentViewModel? assessmentViewModel;

    [ObservableProperty]
    private PlanViewModel? planViewModel;

    /// <summary>
    /// 便捷布尔属性：当前是否是“心理咨询”标签
    /// </summary>
    public bool IsChatTabSelected => SelectedTabIndex == 0;

    /// <summary>
    /// 非聊天模块的选中会话标识（占位以支持数据结构扩展）
    /// </summary>
    [ObservableProperty]
    private string? selectedAssessmentSessionId;

    [ObservableProperty]
    private string? selectedPlanSessionId;

    partial void OnCurrentModuleChanged(string value)
    {
        // 根据模块路由键同步选中索引
        SelectedTabIndex = value switch
        {
            var v when v == SessionType.Chat => 0,
            var v when v == SessionType.Assessment => 1,
            var v when v == SessionType.Plan => 2,
            _ => 0
        };
    }

    partial void OnSelectedTabIndexChanged(int value)
    {
        // 根据索引同步模块路由键
        CurrentModule = value switch
        {
            0 => SessionType.Chat,
            1 => SessionType.Assessment,
            2 => SessionType.Plan,
            _ => SessionType.Chat
        };
        OnPropertyChanged(nameof(IsChatTabSelected));
    }

    // 供顶部栏 Tab 按钮调用
    [RelayCommand]
    private void SelectChatTab() => SelectedTabIndex = 0;

    [RelayCommand]
    private void SelectAssessmentTab() => SelectedTabIndex = 1;

    [RelayCommand]
    private void SelectPlanTab() => SelectedTabIndex = 2;

    [RelayCommand]
    private void PreviousTab()
    {
        if (SelectedTabIndex > 0) SelectedTabIndex--;
    }

    [RelayCommand]
    private void NextTab()
    {
        if (SelectedTabIndex < 2) SelectedTabIndex++;
    }

    [RelayCommand]
    private void Back()
    {
        if (IsSettingsViewVisible)
        {
            IsSettingsViewVisible = false;
            return;
        }
        if (IsSiderOpen)
        {
            IsSiderOpen = false;
            return;
        }
        if (SelectedTabIndex > 0)
        {
            SelectedTabIndex--;
        }
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

        // 其他模块 VM
        AssessmentViewModel = new AssessmentViewModel(App.Services.GetRequiredService<IApiService>());
        PlanViewModel = new PlanViewModel(App.Services.GetRequiredService<IApiService>());

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
            SessionType.Chat
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

    // 通过 TabControl 控制模块切换，故移除显式切换命令

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
        // 仅创建聊天类型会话
        string sessionType = SessionType.Chat;

        // 创建会话
        var session = await _historyService.CreateSessionAsync(
            $"新会话 {Chats.Count + 1}",
            sessionType
        );

        if (sessionType == SessionType.Chat)
        {
            // 创建视图模型
            var newChat = new ChatViewModel() { ChatId = session.Id, ChatTitle = session.Title };
            Chats.Add(newChat);
            _chatViewModelMap[session.Id] = newChat;
            CurrentChat = newChat;
        }
        else if (sessionType == SessionType.Assessment)
        {
            SelectedAssessmentSessionId = session.Id;
        }
        else if (sessionType == SessionType.Plan)
        {
            SelectedPlanSessionId = session.Id;
        }

        // 刷新历史记录
        await LoadHistoryGroupsAsync();

        // 关闭侧边栏
        IsSiderOpen = false;
    }

    // 顶栏标签切换命令


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
