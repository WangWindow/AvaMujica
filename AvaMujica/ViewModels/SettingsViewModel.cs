using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using AvaMujica.Models;
using AvaMujica.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaMujica.ViewModels;

#pragma warning disable CS8604 // 引用类型参数可能为 null。

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

    [ObservableProperty]
    private string _systemPrompt = string.Empty;

    [ObservableProperty]
    private string _apiBase = string.Empty;

    [ObservableProperty]
    private float _temperature = 1.2f;

    [ObservableProperty]
    private int _maxTokens = 2000;

    /// <summary>
    /// API Key编辑状态
    /// </summary>
    [ObservableProperty]
    private bool _isApiKeyEditing = false;

    /// <summary>
    /// API设置编辑状态
    /// </summary>
    [ObservableProperty]
    private bool _isApiSettingsEditing = false;

    /// <summary>
    /// 临时API Key值
    /// </summary>
    [ObservableProperty]
    private string _tempApiKey = string.Empty;

    /// <summary>
    /// 临时系统提示词
    /// </summary>
    [ObservableProperty]
    private string _tempSystemPrompt = string.Empty;

    /// <summary>
    /// 临时API基础URL
    /// </summary>
    [ObservableProperty]
    private string _tempApiBase = string.Empty;

    /// <summary>
    /// 临时温度参数
    /// </summary>
    [ObservableProperty]
    private string _tempTemperature = string.Empty;

    /// <summary>
    /// 临时最大Token数
    /// </summary>
    [ObservableProperty]
    private string _tempMaxTokens = string.Empty;

    /// <summary>
    /// 是否显示 Reasoning
    /// </summary>
    [ObservableProperty]
    private bool _isShowReasoning = true;

    /// <summary>
    /// IsShowReasoning属性变化时触发
    /// </summary>
    partial void OnIsShowReasoningChanged(bool value)
    {
        // 保存到数据库
        _configService.SetConfig("IsShowReasoning", value.ToString());

        // 通知所有 ChatViewModel 更新设置
        foreach (var chat in _mainViewModel.Chats)
        {
            chat.LoadConfiguration();
        }
    }

    /// <summary>
    /// 模型显示名称映射
    /// </summary>
    private readonly Dictionary<string, string> _modelDisplayMapping = new()
    {
        { ChatModels.Chat, "Chat" },
        { ChatModels.Reasoner, "Reasoner" }
    };

    /// <summary>
    /// 显示名称到实际模型的反向映射
    /// </summary>
    private readonly Dictionary<string, string> _displayToModelMapping = new()
    {
        { "Chat", ChatModels.Chat },
        { "Reasoner", ChatModels.Reasoner }
    };

    /// <summary>
    /// 可用模型显示列表
    /// </summary>
    public ObservableCollection<string> AvailableModels { get; } =
        new() { "Chat", "Reasoner" };

    /// <summary>
    /// 显示用的选中模型
    /// </summary>
    [ObservableProperty]
    private string _selectedDisplayModel = string.Empty;

    /// <summary>
    /// 初始化
    /// </summary>
    public void Initialize()
    {
        // 从配置中加载API Key和模型设置
        var config = _configService.LoadFullConfig();
        ApiKey = config.ApiKey;
        SelectedModel = config.Model;

        // 设置显示模型
        if (_modelDisplayMapping.TryGetValue(config.Model, out var displayName))
        {
            SelectedDisplayModel = displayName;
        }
        else
        {
            SelectedDisplayModel = "Chat"; // 默认值
        }

        SystemPrompt = config.SystemPrompt;
        ApiBase = config.ApiBase;
        Temperature = config.Temperature;
        MaxTokens = config.MaxTokens;
        IsShowReasoning = config.IsShowReasoning;
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
    /// 打开API Key编辑
    /// </summary>
    [RelayCommand]
    private void SetApiKey()
    {
        // 使用内联编辑方式
        IsApiKeyEditing = true;
    }

    /// <summary>
    /// 打开API设置编辑
    /// </summary>
    [RelayCommand]
    private void SetApiSettings()
    {
        // 使用内联编辑方式
        IsApiSettingsEditing = true;
    }

    /// <summary>
    /// 保存API Key
    /// </summary>
    [RelayCommand]
    private void SaveApiKey()
    {
        if (!string.IsNullOrEmpty(TempApiKey))
        {
            _configService.SetConfig("ApiKey", TempApiKey);
            ApiKey = TempApiKey;
        }
        IsApiKeyEditing = false;
    }

    /// <summary>
    /// 取消API Key编辑
    /// </summary>
    [RelayCommand]
    private void CancelApiKeyEdit()
    {
        IsApiKeyEditing = false;
    }

    /// <summary>
    /// 保存API设置
    /// </summary>
    [RelayCommand]
    private void SaveApiSettings()
    {
        // 保存系统提示词
        if (!string.IsNullOrEmpty(TempSystemPrompt))
        {
            _configService.SetConfig("SystemPrompt", TempSystemPrompt);
            SystemPrompt = TempSystemPrompt;
        }

        // 保存API基础URL
        var newApiBase = string.IsNullOrEmpty(TempApiBase)
            ? "https://api.deepseek.com"
            : TempApiBase;
        _configService.SetConfig("ApiBase", newApiBase);
        ApiBase = newApiBase;

        // 保存温度参数
        if (float.TryParse(TempTemperature, out float newTemperature))
        {
            // 限制温度范围
            newTemperature = Math.Clamp(newTemperature, 0.0f, 2.0f);
            _configService.SetConfig("Temperature", newTemperature.ToString());
            Temperature = newTemperature;
        }

        // 保存最大token数
        if (int.TryParse(TempMaxTokens, out int newMaxTokens))
        {
            // 确保最大token数合理
            newMaxTokens = Math.Max(1, newMaxTokens);
            _configService.SetConfig("MaxTokens", newMaxTokens.ToString());
            MaxTokens = newMaxTokens;
        }

        IsApiSettingsEditing = false;
    }

    /// <summary>
    /// 取消API设置编辑
    /// </summary>
    [RelayCommand]
    private void CancelApiSettingsEdit()
    {
        IsApiSettingsEditing = false;
    }

    /// <summary>
    /// 准备API Key编辑
    /// </summary>
    private void PrepareApiKeyEdit()
    {
        TempApiKey = ApiKey;
    }

    /// <summary>
    /// 准备API设置编辑
    /// </summary>
    private void PrepareApiSettingsEdit()
    {
        TempSystemPrompt = SystemPrompt;
        TempApiBase = ApiBase;
        TempTemperature = Temperature.ToString();
        TempMaxTokens = MaxTokens.ToString();
    }

    /// <summary>
    /// API Key编辑状态改变
    /// </summary>
    partial void OnIsApiKeyEditingChanged(bool value)
    {
        if (value)
        {
            PrepareApiKeyEdit();
        }
    }

    /// <summary>
    /// API设置编辑状态改变
    /// </summary>
    partial void OnIsApiSettingsEditingChanged(bool value)
    {
        if (value)
        {
            PrepareApiSettingsEdit();
        }
    }

    /// <summary>
    /// 选择模型改变时触发（接收显示名称）
    /// </summary>
    [RelayCommand]
    private void SelectModel(string displayModel)
    {
        if (_displayToModelMapping.TryGetValue(displayModel, out var actualModel))
        {
            SelectedModel = actualModel;
            SelectedDisplayModel = displayModel;
            // 保存到数据库
            _configService.SetConfig("Model", actualModel);
        }
    }

    /// <summary>
    /// 切换显示思考过程
    /// </summary>
    [RelayCommand]
    private void ToggleShowReasoning()
    {
        IsShowReasoning = !IsShowReasoning;
    }
}
