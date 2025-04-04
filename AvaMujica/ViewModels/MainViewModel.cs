using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AvaMujica.Models;
using AvaMujica.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaMujica.ViewModels;

/// <summary>
/// 主视图模型
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly ChatService _chatService = ChatService.Instance;
    private readonly HistoryService _historyService = HistoryService.Instance;

    /// <summary>
    /// SessionID 到 ChatViewModel 的映射字典，用于快速查找
    /// </summary>
    private readonly Dictionary<string, ChatViewModel> _chatViewModelMap = new();

    /// <summary>
    /// 窗口标题
    /// </summary>
    public string Title { get; set; } = "AvaMujica";

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
    public ObservableCollection<ChatSessionGroup> ChatSessionGroups { get; } = [];

    public MainViewModel()
    {
        try
        {
            // 初始化视图模型
            SiderViewModel = new SiderViewModel(this);
            SettingsViewModel = new SettingsViewModel(this);

            // 异步初始化数据
            _ = InitializeAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"MainViewModel初始化失败: {ex.Message}");
            // 确保在出现异常时仍然初始化所需属性
            SiderViewModel ??= new SiderViewModel(this);
            SettingsViewModel ??= new SettingsViewModel(this);
        }
    }

    /// <summary>
    /// 初始化ViewModel
    /// </summary>
    private async Task InitializeAsync()
    {
        try
        {
            await LoadChatsAsync();
            await LoadHistoryGroupsAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"初始化数据失败: {ex.Message}");
            // 可以在这里添加重试逻辑或显示错误信息给用户
        }
    }

    /// <summary>
    /// 加载聊天会话
    /// </summary>
    private async Task LoadChatsAsync()
    {
        try
        {
            var sessions = await _chatService.GetAllSessionsAsync();

            Chats.Clear();
            _chatViewModelMap.Clear();

            foreach (var session in sessions)
            {
                var chatViewModel = new ChatViewModel()
                {
                    ChatId = session.Id,
                    ChatTitle = session.Title,
                    StartTime = session.CreatedTime.ToString("HH:mm"),
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
        catch (Exception ex)
        {
            // 处理异常，可以记录日志或显示给用户
            Console.WriteLine($"加载聊天会话失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 加载历史记录分组
    /// </summary>
    private async Task LoadHistoryGroupsAsync()
    {
        try
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
                        item.ChatViewModel = chatViewModel;
                    }
                }

                ChatSessionGroups.Add(group);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"加载历史记录失败: {ex.Message}");
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
    public async Task CreateNewChatAsync()
    {
        try
        {
            // 确定会话类型
            string sessionType = ChatSessionType.PsychologicalConsultation;
            if (IsAssessmentModuleSelected)
                sessionType = ChatSessionType.PsychologicalAssessment;
            else if (IsInterventionModuleSelected)
                sessionType = ChatSessionType.InterventionPlan;

            // 创建会话
            var session = await _chatService.CreateSessionAsync(
                $"新会话 {Chats.Count + 1}",
                sessionType
            );

            // 创建视图模型
            var newChat = new ChatViewModel()
            {
                ChatId = session.Id,
                ChatTitle = session.Title,
                StartTime = session.CreatedTime.ToString("HH:mm"),
            };

            Chats.Add(newChat);
            _chatViewModelMap[session.Id] = newChat;
            CurrentChat = newChat;

            // 刷新历史记录
            await LoadHistoryGroupsAsync();

            // 关闭侧边栏
            IsSiderOpen = false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"创建新会话失败: {ex.Message}");
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

    /// <summary>
    /// 根据ID切换当前会话
    /// </summary>
    /// <param name="sessionId">会话ID</param>
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
    /// 删除会话
    /// </summary>
    public async Task DeleteChatAsync(string sessionId)
    {
        try
        {
            await _chatService.DeleteSessionAsync(sessionId);

            // 从列表和映射中移除
            if (_chatViewModelMap.TryGetValue(sessionId, out var chat))
            {
                Chats.Remove(chat);
                _chatViewModelMap.Remove(sessionId);

                // 如果删除的是当前会话，切换到其他会话
                if (CurrentChat == chat)
                {
                    if (Chats.Count > 0)
                    {
                        CurrentChat = Chats[0];
                    }
                    else
                    {
                        // 没有会话了，创建一个新会话
                        await CreateNewChatAsync();
                    }
                }
            }

            // 刷新历史记录
            await LoadHistoryGroupsAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"删除会话失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 根据会话ID获取对应的ChatViewModel
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <returns>对应的ChatViewModel，如果不存在返回null</returns>
    public ChatViewModel? GetChatViewModel(string sessionId)
    {
        _chatViewModelMap.TryGetValue(sessionId, out var chatViewModel);
        return chatViewModel;
    }
}
