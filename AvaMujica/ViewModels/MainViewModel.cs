using System;
using System.Collections.ObjectModel;
using System.Linq;
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
    private readonly MyApi _api = new();

    /// <summary>
    /// 聊天管理器
    /// </summary>
    private ChatManager _chatManager = new();

    /// <summary>
    /// 历史记录管理器
    /// </summary>
    public HistoryManager _historyManager = new();

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
    /// 是否选择了心理咨询模块（默认选中）
    /// </summary>
    [ObservableProperty]
    private bool isConsultationModuleSelected = true;

    /// <summary>
    /// 是否选择了心理评估模块
    /// </summary>
    [ObservableProperty]
    private bool isAssessmentModuleSelected = false;

    /// <summary>
    /// 是否选择了干预方案模块
    /// </summary>
    [ObservableProperty]
    private bool isInterventionModuleSelected = false;

    /// <summary>
    /// 所有对话列表
    /// </summary>
    public ObservableCollection<ChatViewModel> Chats { get; } = [];

    /// <summary>
    /// 历史记录集合（按日期分组）
    /// </summary>
    public ObservableCollection<HistoryGroup> HistoryGroups { get; } = [];

    public MainViewModel()
    {
        SiderViewModel = new SiderViewModel(this);
        SettingsViewModel = new SettingsViewModel(this);

        InitializeChatManager(); // 初始化聊天管理器
        InitializeHistoryManager(); // 初始化历史记录管理器
    }

    /// <summary>
    /// 初始化聊天管理器
    /// </summary>
    private void InitializeChatManager()
    {
        // 加载已有会话并转换为ChatViewModel
        foreach (var session in _chatManager.Sessions)
        {
            var chatViewModel = new ChatViewModel(session);
            Chats.Add(chatViewModel);
        }

        // 如果有会话，选择第一个作为当前会话
        if (Chats.Count > 0)
        {
            CurrentChat = Chats[0];
        }
        else
        {
            // 没有会话则创建新会话
            CreateNewChat();
        }
    }

    /// <summary>
    /// 初始化历史记录管理器
    /// </summary>
    private void InitializeHistoryManager()
    {
        // 根据已有的会话创建历史记录
        foreach (var chat in Chats)
        {
            var historyInfo = new HistoryInfo
            {
                Id = chat.ChatId,
                Title = chat.ChatTitle,
                Time = chat.StartTime,
                ChatViewModel = chat,
            };

            // 添加到历史记录
            _historyManager.AddHistory(historyInfo);
        }
    }

    /// <summary>
    /// 当心理咨询模块选择状态改变时触发
    /// </summary>
    partial void OnIsConsultationModuleSelectedChanged(bool value)
    {
        if (value)
        {
            IsAssessmentModuleSelected = false;
            IsInterventionModuleSelected = false;
        }
    }

    /// <summary>
    /// 当心理评估模块选择状态改变时触发
    /// </summary>
    partial void OnIsAssessmentModuleSelectedChanged(bool value)
    {
        if (value)
        {
            IsConsultationModuleSelected = false;
            IsInterventionModuleSelected = false;
        }
    }

    /// <summary>
    /// 当干预方案模块选择状态改变时触发
    /// </summary>
    partial void OnIsInterventionModuleSelectedChanged(bool value)
    {
        if (value)
        {
            IsConsultationModuleSelected = false;
            IsAssessmentModuleSelected = false;
        }
    }

    /// <summary>
    /// 切换到心理咨询模块
    /// </summary>
    [RelayCommand]
    public void SwitchToConsultationModule()
    {
        IsConsultationModuleSelected = true;
        IsSettingsViewVisible = false;
    }

    /// <summary>
    /// 切换到心理评估模块
    /// </summary>
    [RelayCommand]
    public void SwitchToAssessmentModule()
    {
        IsAssessmentModuleSelected = true;
        IsSettingsViewVisible = false;
    }

    /// <summary>
    /// 切换到干预方案模块
    /// </summary>
    [RelayCommand]
    public void SwitchToInterventionModule()
    {
        IsInterventionModuleSelected = true;
        IsSettingsViewVisible = false;
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
    public void CreateNewChat()
    {
        // 使用聊天管理器创建新会话
        var session = _chatManager.CreateNewSession($"新对话 {Chats.Count + 1}");

        // 创建新的聊天视图模型
        var newChat = new ChatViewModel(session)
        {
            ChatTitle = session.Title,
            StartTime = session.CreatedAt,
        };

        Chats.Add(newChat);
        CurrentChat = newChat;

        // 创建并添加历史记录
        var historyInfo = new HistoryInfo
        {
            Id = newChat.ChatId,
            Title = newChat.ChatTitle,
            Time = newChat.StartTime,
            ChatViewModel = newChat,
        };

        _historyManager.AddHistory(historyInfo);

        // 关闭侧边栏，让用户可以立即开始新对话
        IsSiderOpen = false;
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
