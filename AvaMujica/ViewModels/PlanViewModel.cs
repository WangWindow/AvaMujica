using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AvaMujica.Models;
using AvaMujica.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaMujica.ViewModels;

public partial class PlanViewModel : ViewModelBase
{
    private readonly IApiService _apiService;

    public ObservableCollection<PlanItem> Items { get; } = new();

    [ObservableProperty]
    private string newItemTitle = string.Empty;

    [ObservableProperty]
    private string? selectedItemId;

    public PlanViewModel(IApiService apiService)
    {
        _apiService = apiService;
    }

    [RelayCommand]
    private void AddItem()
    {
        if (string.IsNullOrWhiteSpace(NewItemTitle)) return;
        Items.Add(new PlanItem { Title = NewItemTitle });
        NewItemTitle = string.Empty;
    }

    [RelayCommand]
    private void RemoveItem(PlanItem? item)
    {
        if (item is null) return;
        Items.Remove(item);
    }

    [RelayCommand]
    private void ToggleDone(PlanItem? item)
    {
        if (item is null) return;
        item.IsDone = !item.IsDone;
        OnPropertyChanged(nameof(Items));
    }

    [RelayCommand]
    private async Task GenerateByAi()
    {
        // 基于当前项生成完善建议，或生成一个初始方案
        string prompt = "请根据用户的心理健康改善目标，给出一个包含5-7条可执行、可度量且温和的日常干预建议（中文、短句、编号省略）。";
        var buffer = string.Empty;
        await _apiService.ChatAsync(
            prompt,
            async (t, s) =>
            {
                if (t == ResponseType.Content)
                {
                    buffer += s;
                }
                await Task.CompletedTask;
            }
        );

        // 简单解析为多行待办
        var lines = buffer
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.TrimStart('-', '*', ' ', '\t'))
            .Where(l => l.Length > 0)
            .Take(10)
            .ToList();

        if (lines.Count == 0)
        {
            lines = new()
            {
                "保证规律作息，固定上床与起床时间",
                "每日进行10-20分钟的轻度运动，如散步",
                "安排 10 分钟呼吸放松或正念练习",
                "减少睡前 1 小时电子设备使用",
                "与家人朋友保持沟通，分享感受",
                "记录每天一件小确幸，培养积极情绪"
            };
        }

        Items.Clear();
        foreach (var l in lines)
        {
            Items.Add(new PlanItem { Title = l });
        }
    }
}
