using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AvaMujica.Models;
using AvaMujica.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaMujica.ViewModels;

/// <summary>
/// 设置视图模型
/// </summary>
public partial class SettingsViewModel : ViewModelBase
{
    /// <summary>
    /// 主视图模型
    /// </summary>
    private readonly MainViewModel _mainViewModel;

    /// <summary>
    /// 配置服务
    /// </summary>
    private readonly ConfigService _configService;

    /// <summary>
    /// API密钥
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsApiKeyValid))]
    [NotifyCanExecuteChangedFor(nameof(SaveConfigCommand))]
    private string apiKey = string.Empty;

    /// <summary>
    /// API基础地址
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsApiBaseValid))]
    [NotifyCanExecuteChangedFor(nameof(SaveConfigCommand))]
    private string apiBase = string.Empty;

    /// <summary>
    /// 模型名称
    /// </summary>
    [ObservableProperty]
    private string model = string.Empty;

    /// <summary>
    /// 系统提示
    /// </summary>
    [ObservableProperty]
    private string systemPrompt = string.Empty;

    /// <summary>
    /// 是否显示推理过程
    /// </summary>
    [ObservableProperty]
    private bool showReasoning = true;

    /// <summary>
    /// 温度参数
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsTemperatureValid))]
    [NotifyCanExecuteChangedFor(nameof(SaveConfigCommand))]
    private float temperature = 1.3f;

    /// <summary>
    /// 最大令牌数
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsMaxTokensValid))]
    [NotifyCanExecuteChangedFor(nameof(SaveConfigCommand))]
    private int maxTokens = 2000;

    /// <summary>
    /// 保存配置是否成功
    /// </summary>
    [ObservableProperty]
    private bool isSaveSuccessful = false;

    /// <summary>
    /// 保存配置是否失败
    /// </summary>
    [ObservableProperty]
    private bool isSaveFailed = false;

    /// <summary>
    /// 错误消息
    /// </summary>
    [ObservableProperty]
    private string errorMessage = string.Empty;

    /// <summary>
    /// 可选的模型列表
    /// </summary>
    public List<string> AvailableModels { get; } =
        new List<string> { "deepseek-reasoner", "deepseek-chat", "deepseek-llm", "deepseek-coder" };

    /// <summary>
    /// 可选的预设提示列表
    /// </summary>
    public List<string> PresetPrompts { get; } =
        new List<string>
        {
            "你是一名优秀的心理咨询师，具有丰富的咨询经验。你的工作是为用户提供情感支持，解决用户的疑问。",
            "你是一名专业的心理评估师，善于分析用户心理状态并提供客观评估。",
            "你是一名心理干预专家，可以为用户提供科学有效的干预方案。",
            "你是一名心理健康教育者，擅长普及心理健康知识。",
        };

    /// <summary>
    /// API密钥是否有效
    /// </summary>
    public bool IsApiKeyValid => !string.IsNullOrWhiteSpace(ApiKey);

    /// <summary>
    /// API基础地址是否有效
    /// </summary>
    public bool IsApiBaseValid => Uri.TryCreate(ApiBase, UriKind.Absolute, out _);

    /// <summary>
    /// 温度参数是否有效
    /// </summary>
    public bool IsTemperatureValid => Temperature >= 0 && Temperature <= 2;

    /// <summary>
    /// 最大令牌数是否有效
    /// </summary>
    public bool IsMaxTokensValid => MaxTokens >= 100 && MaxTokens <= 100000;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="mainViewModel"></param>
    public SettingsViewModel(MainViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;

        // 使用ServiceCollection获取服务
        var services = ServiceCollection.Instance;
        _configService = services.ConfigService;
        // 加载配置
        LoadConfig();
    }

    // 无参构造函数，用于XAML设计时
    public SettingsViewModel()
        : this(null!) { }

    /// <summary>
    /// 加载配置
    /// </summary>
    private void LoadConfig()
    {
        try
        {
            var config = _configService.LoadFullConfig();

            ApiKey = config.ApiKey;
            ApiBase = config.ApiBase;
            Model = config.Model;
            SystemPrompt = config.SystemPrompt;
            ShowReasoning = config.ShowReasoning;
            Temperature = config.Temperature;
            MaxTokens = config.MaxTokens;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"加载配置失败: {ex.Message}";
            IsSaveFailed = true;

            // 5秒后清除错误消息
            Task.Delay(5000)
                .ContinueWith(_ =>
                {
                    ErrorMessage = string.Empty;
                    IsSaveFailed = false;
                });
        }
    }

    /// <summary>
    /// 保存配置
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSaveConfig))]
    private void SaveConfig()
    {
        try
        {
            // 验证输入
            if (!IsApiBaseValid)
            {
                ErrorMessage = "API基础地址格式不正确";
                IsSaveFailed = true;
                return;
            }

            if (!IsTemperatureValid)
            {
                ErrorMessage = "温度参数应在0-2之间";
                IsSaveFailed = true;
                return;
            }

            if (!IsMaxTokensValid)
            {
                ErrorMessage = "最大令牌数应在100-100000之间";
                IsSaveFailed = true;
                return;
            }

            var config = new Config
            {
                ApiKey = ApiKey.Trim(),
                ApiBase = ApiBase.Trim(),
                Model = Model,
                SystemPrompt = SystemPrompt,
                ShowReasoning = ShowReasoning,
                Temperature = Temperature,
                MaxTokens = MaxTokens,
            };

            _configService.SaveFullConfig(config);
            IsSaveSuccessful = true;
            IsSaveFailed = false;

            // 2秒后重置状态
            Task.Delay(2000)
                .ContinueWith(_ =>
                {
                    IsSaveSuccessful = false;
                });
        }
        catch (Exception ex)
        {
            ErrorMessage = $"保存配置失败: {ex.Message}";
            IsSaveFailed = true;
            IsSaveSuccessful = false;

            // 5秒后清除错误消息
            Task.Delay(5000)
                .ContinueWith(_ =>
                {
                    ErrorMessage = string.Empty;
                    IsSaveFailed = false;
                });
        }
    }

    /// <summary>
    /// 选择预设提示语
    /// </summary>
    [RelayCommand]
    private void SelectPresetPrompt(string prompt)
    {
        if (!string.IsNullOrEmpty(prompt))
        {
            SystemPrompt = prompt;
        }
    }

    /// <summary>
    /// 重置为默认配置
    /// </summary>
    [RelayCommand]
    private void ResetToDefaults()
    {
        ApiKey = Environment.GetEnvironmentVariable("DEEPSEEK_API_KEY") ?? string.Empty;
        ApiBase = "https://api.deepseek.com";
        Model = "deepseek-reasoner";
        SystemPrompt =
            "你是一名优秀的心理咨询师，具有丰富的咨询经验。你的工作是为用户提供情感支持，解决用户的疑问。";
        ShowReasoning = true;
        Temperature = 1.3f;
        MaxTokens = 2000;
    }

    /// <summary>
    /// 判断是否可以保存配置
    /// </summary>
    private bool CanSaveConfig()
    {
        return IsApiBaseValid && IsTemperatureValid && IsMaxTokensValid;
    }

    /// <summary>
    /// 返回聊天界面
    /// </summary>
    [RelayCommand]
    private void GoBack()
    {
        _mainViewModel?.ReturnToChat();
    }
}
