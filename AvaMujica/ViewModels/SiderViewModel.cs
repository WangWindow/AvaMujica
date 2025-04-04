using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AvaMujica.Models;
using AvaMujica.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaMujica.ViewModels;

/// <summary>
/// 侧边栏视图模型
/// </summary>
public partial class SiderViewModel : ViewModelBase
{
    private readonly ChatService _chatService = ChatService.Instance;
    private readonly HistoryService _historyService = HistoryService.Instance;

    /// <summary>
    /// 主视图模型引用
    /// </summary>
    private readonly MainViewModel _mainViewModel;

    /// <summary>
    /// 历史记录分组
    /// </summary>
    public ObservableCollection<ChatSessionGroup> ChatSessionGroups =>
        _mainViewModel.ChatSessionGroups;

    /// <summary>
    /// 当前选中的会话类型
    /// </summary>
    [ObservableProperty]
    private string selectedSessionType = ChatSessionType.PsychologicalConsultation;

    /// <summary>
    /// 是否正在加载
    /// </summary>
    [ObservableProperty]
    private bool isLoading = false;

    /// <summary>
    /// 是否有错误
    /// </summary>
    [ObservableProperty]
    private bool hasError = false;

    /// <summary>
    /// 错误信息
    /// </summary>
    [ObservableProperty]
    private string errorMessage = string.Empty;

    /// <summary>
    /// 是否有会话
    /// </summary>
    [ObservableProperty]
    private bool hasChats = false;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="mainViewModel">主视图模型</param>
    public SiderViewModel(MainViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;

        // 设置属性更改通知
        this.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(ChatSessionGroups))
            {
                HasChats = ChatSessionGroups.Any(g => g.Items.Count > 0);
            }
        };

        // 初始加载会话
        _ = RefreshHistoryAsync();
    }

    /// <summary>
    /// 当会话类型选择改变时触发
    /// </summary>
    partial void OnSelectedSessionTypeChanged(string value)
    {
        _ = FilterHistoryByTypeAsync(value);
    }

    /// <summary>
    /// 根据类型筛选历史记录
    /// </summary>
    private async Task FilterHistoryByTypeAsync(string sessionType)
    {
        try
        {
            IsLoading = true;
            HasError = false;

            List<ChatSessionGroup> historyGroups;

            try
            {
                historyGroups = sessionType switch
                {
                    ChatSessionType.PsychologicalConsultation =>
                        await _historyService.GetChatSessionHistorysByTypeAsync(
                            ChatSessionType.PsychologicalConsultation
                        ),
                    ChatSessionType.PsychologicalAssessment =>
                        await _historyService.GetChatSessionHistorysByTypeAsync(
                            ChatSessionType.PsychologicalAssessment
                        ),
                    ChatSessionType.InterventionPlan =>
                        await _historyService.GetChatSessionHistorysByTypeAsync(
                            ChatSessionType.InterventionPlan
                        ),
                    _ => await _historyService.GetChatSessionHistorysByTypeAsync(
                        ChatSessionType.PsychologicalConsultation
                    ),
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取历史记录失败: {ex.Message}");
                historyGroups = [];
            }

            _mainViewModel.ChatSessionGroups.Clear();
            foreach (var group in historyGroups)
            {
                _mainViewModel.ChatSessionGroups.Add(group);
            }

            HasChats = _mainViewModel.ChatSessionGroups.Any(g => g.Items.Count > 0);
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"筛选历史记录失败: {ex.Message}";
            Console.WriteLine(ErrorMessage);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// 刷新历史记录
    /// </summary>
    [RelayCommand]
    private async Task RefreshHistoryAsync()
    {
        await FilterHistoryByTypeAsync(SelectedSessionType);
    }

    /// <summary>
    /// 创建新会话命令
    /// </summary>
    [RelayCommand]
    private async Task CreateNewChatAsync()
    {
        try
        {
            IsLoading = true;
            HasError = false;
            await _mainViewModel.CreateNewChatAsync();
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"创建新会话失败: {ex.Message}";
            Console.WriteLine(ErrorMessage);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// 打开设置命令
    /// </summary>
    [RelayCommand]
    private void OpenSettings()
    {
        _mainViewModel.ShowSettings();
    }

    /// <summary>
    /// 选择历史会话命令
    /// </summary>
    [RelayCommand]
    private void SelectHistoryItem(ChatSession chatSession)
    {
        if (chatSession != null)
        {
            _mainViewModel.SwitchToChat(chatSession.Id);
        }
    }

    /// <summary>
    /// 删除历史会话命令
    /// </summary>
    [RelayCommand]
    private async Task DeleteHistoryItemAsync(ChatSession chatSession)
    {
        if (chatSession == null)
            return;

        try
        {
            IsLoading = true;
            HasError = false;

            // 通过主视图模型删除会话，确保UI状态一致
            await _mainViewModel.DeleteChatAsync(chatSession.Id);

            // 刷新列表
            await RefreshHistoryAsync();
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"删除会话失败: {ex.Message}";
            Console.WriteLine(ErrorMessage);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// 清除所有会话命令
    /// </summary>
    [RelayCommand]
    private async Task ClearAllHistoryAsync()
    {
        try
        {
            IsLoading = true;
            HasError = false;

            // 获取所有会话
            var sessions = await _chatService.GetAllSessionsAsync();

            // 逐个删除会话
            foreach (var session in sessions)
            {
                await _chatService.DeleteSessionAsync(session.Id);
            }

            // 刷新历史记录
            await RefreshHistoryAsync();

            // 创建新会话
            await _mainViewModel.CreateNewChatAsync();
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"清空历史记录失败: {ex.Message}";
            Console.WriteLine(ErrorMessage);
        }
        finally
        {
            IsLoading = false;
        }
    }
}
