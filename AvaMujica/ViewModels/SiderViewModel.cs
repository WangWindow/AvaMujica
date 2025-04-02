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
    /// <summary>
    /// 主视图模型引用
    /// </summary>
    private readonly MainViewModel _mainViewModel;

    /// <summary>
    /// 聊天服务
    /// </summary>
    private readonly ChatService _chatService;

    /// <summary>
    /// 历史记录分组
    /// </summary>
    public ObservableCollection<HistoryGroup> HistoryGroups => _mainViewModel.HistoryGroups;

    /// <summary>
    /// 当前选中的会话类型
    /// </summary>
    [ObservableProperty]
    private string selectedSessionType = "所有会话";

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
    /// 可用的会话类型
    /// </summary>
    public List<string> SessionTypes { get; } =
        new List<string> { "所有会话", "心理咨询会话", "心理评估会话", "干预方案会话" };

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="mainViewModel">主视图模型</param>
    public SiderViewModel(MainViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;

        // 使用ServiceCollection获取服务
        var services = ServiceCollection.Instance;
        _chatService = services.ChatService;

        // 设置属性更改通知
        this.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(HistoryGroups))
            {
                HasChats = HistoryGroups.Any(g => g.Items.Count > 0);
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

            List<HistoryGroup> historyGroups;

            try
            {
                historyGroups = sessionType switch
                {
                    "所有会话" => await _chatService.GetHistoryGroupsAsync(),
                    "心理咨询会话" => await _chatService.GetHistoryGroupsByTypeAsync("咨询"),
                    "心理评估会话" => await _chatService.GetHistoryGroupsByTypeAsync("评估"),
                    "干预方案会话" => await _chatService.GetHistoryGroupsByTypeAsync("干预"),
                    _ => await _chatService.GetHistoryGroupsAsync(),
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取历史记录失败: {ex.Message}");
                historyGroups = new List<HistoryGroup>();
            }

            _mainViewModel.HistoryGroups.Clear();
            foreach (var group in historyGroups)
            {
                _mainViewModel.HistoryGroups.Add(group);
            }

            HasChats = _mainViewModel.HistoryGroups.Any(g => g.Items.Count > 0);
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
    private void SelectHistoryItem(HistoryInfo historyInfo)
    {
        if (historyInfo != null)
        {
            _mainViewModel.SwitchToChat(historyInfo.Id);
        }
    }

    /// <summary>
    /// 删除历史会话命令
    /// </summary>
    [RelayCommand]
    private async Task DeleteHistoryItemAsync(HistoryInfo historyInfo)
    {
        if (historyInfo == null)
            return;

        try
        {
            IsLoading = true;
            HasError = false;

            // 通过主视图模型删除会话，确保UI状态一致
            await _mainViewModel.DeleteChatAsync(historyInfo.Id);

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
