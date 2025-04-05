using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
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
        SystemPrompt = config.SystemPrompt;
        ApiBase = config.ApiBase;
        Temperature = config.Temperature;
        MaxTokens = config.MaxTokens;
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
        var result = await ShowInputDialog("API Key 设置", "请输入您的API Key:", dialog);

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
    /// 打开API设置对话框
    /// </summary>
    [RelayCommand]
    private async Task SetApiSettings()
    {
        // 创建系统提示词输入框
        var systemPromptTextBox = new TextBox
        {
            AcceptsReturn = true,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            MinHeight = 100,
            MaxHeight = 200,
            Watermark = "请输入系统提示词",
            Text = SystemPrompt,
        };

        // 创建API基础URL输入框
        var apiBaseTextBox = new TextBox { Watermark = "请输入API基础URL", Text = ApiBase };

        // 创建温度参数输入框
        var temperatureTextBox = new TextBox
        {
            Watermark = "请输入温度参数 (0.0-2.0)",
            Text = Temperature.ToString(),
        };

        // 创建最大Token输入框
        var maxTokensTextBox = new TextBox
        {
            Watermark = "请输入最大生成token数",
            Text = MaxTokens.ToString(),
        };

        // 创建确认和取消按钮
        var okButton = new Button { Content = "确认" };
        var cancelButton = new Button { Content = "取消" };

        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            Spacing = 10,
            Children = { cancelButton, okButton },
        };

        // 创建标签
        var systemPromptLabel = new TextBlock { Text = "系统提示词:" };
        var apiBaseLabel = new TextBlock { Text = "API基础URL:" };
        var temperatureLabel = new TextBlock { Text = "温度参数:" };
        var maxTokensLabel = new TextBlock { Text = "最大Token数:" };

        // 创建设置内容面板
        var settingsPanel = new StackPanel
        {
            Margin = new Avalonia.Thickness(20),
            Spacing = 10,
            Children =
            {
                systemPromptLabel,
                systemPromptTextBox,
                apiBaseLabel,
                apiBaseTextBox,
                temperatureLabel,
                temperatureTextBox,
                maxTokensLabel,
                maxTokensTextBox,
                buttonPanel,
            },
        };

        // 创建对话框窗口
        var dialog = new Window
        {
            Width = 500,
            Height = 500,
            Title = "API设置",
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Content = settingsPanel,
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
        bool result = await tcs.Task;

        if (result)
        {
            // 保存系统提示词
            var newSystemPrompt = systemPromptTextBox.Text?.Trim() ?? string.Empty;
            if (!string.IsNullOrEmpty(newSystemPrompt))
            {
                _configService.SetConfig("SystemPrompt", newSystemPrompt);
                SystemPrompt = newSystemPrompt;
            }

            // 保存API基础URL
            var newApiBase = apiBaseTextBox.Text?.Trim() ?? "https://api.deepseek.com";
            _configService.SetConfig("ApiBase", newApiBase);
            ApiBase = newApiBase;

            // 保存温度参数
            if (float.TryParse(temperatureTextBox.Text, out float newTemperature))
            {
                // 限制温度范围
                newTemperature = Math.Clamp(newTemperature, 0.0f, 2.0f);
                _configService.SetConfig("Temperature", newTemperature.ToString());
                Temperature = newTemperature;
            }

            // 保存最大token数
            if (int.TryParse(maxTokensTextBox.Text, out int newMaxTokens))
            {
                // 确保最大token数合理
                newMaxTokens = Math.Max(1, newMaxTokens);
                _configService.SetConfig("MaxTokens", newMaxTokens.ToString());
                MaxTokens = newMaxTokens;
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

    /// <summary>
    /// 显示输入对话框
    /// </summary>
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
