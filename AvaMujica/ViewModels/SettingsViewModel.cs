using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using AvaMujica.Models;
using AvaMujica.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaMujica.ViewModels;

/// <summary>
/// 设置视图模型
/// </summary>
public partial class SettingsViewModel(MainViewModel mainViewModel) : ViewModelBase
{
    /// <summary>
    /// 主视图模型
    /// </summary>
    private readonly MainViewModel _mainViewModel = mainViewModel;

    /// <summary>
    /// 配置服务
    /// </summary>
    private readonly ConfigService _configService = ConfigService.Instance;

    /// <summary>
    /// API服务
    /// </summary>
    private readonly ApiService _apiService = ApiService.Instance;

    [ObservableProperty]
    private string _apiKey = string.Empty;

    [ObservableProperty]
    private string _selectedModel = string.Empty;

    /// <summary>
    /// 可用模型列表
    /// </summary>
    public ObservableCollection<string> AvailableModels { get; } =
        new() { ChatModels.Chat, ChatModels.Reasoner };

    /// <summary>
    /// 初始化
    /// </summary>
    public void Initialize()
    {
        // 从配置中加载API Key和模型设置
        var config = _configService.LoadFullConfig();
        ApiKey = config.ApiKey;
        SelectedModel = config.Model;
    }

    /// <summary>
    /// 返回聊天界面
    /// </summary>
    [RelayCommand]
    private void GoBack()
    {
        _mainViewModel?.ReturnToChat();
    }

    /// <summary>
    /// 保存API Key
    /// </summary>
    [RelayCommand]
    private async Task SetApiKey()
    {
        // 创建输入对话框
        var dialog = new TextBox
        {
            Width = 300,
            Watermark = "请输入API Key",
            Text = ApiKey,
        };

        // 创建对话框
        var result = await _mainViewModel.ShowInputDialog(
            "API Key 设置",
            "请输入您的API Key:",
            dialog
        );

        if (result)
        {
            var newApiKey = dialog.Text?.Trim() ?? string.Empty;
            if (!string.IsNullOrEmpty(newApiKey))
            {
                // 保存到配置
                _configService.SetConfig("ApiKey", newApiKey);
                ApiKey = newApiKey;
            }
        }
    }

    /// <summary>
    /// 选择模型改变时触发
    /// </summary>
    [RelayCommand]
    private void SelectModel(string model)
    {
        SelectedModel = model;

        // 保存到数据库
        _configService.SetConfig("Model", model);
    }
}
