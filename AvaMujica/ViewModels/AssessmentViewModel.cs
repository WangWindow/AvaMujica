using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AvaMujica.Models;
using AvaMujica.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace AvaMujica.ViewModels;

public partial class AssessmentViewModel : ViewModelBase
{
    public AssessmentViewModel(IApiService apiService)
    {
        _apiService = apiService;
        LoadBuiltInScales();
    }

    // 默认构造函数：自动解析单例服务
    public AssessmentViewModel()
        : this(App.Services.GetRequiredService<IApiService>()) { }

    private readonly IApiService _apiService;

    public ObservableCollection<Scale> Scales { get; } = [];

    [ObservableProperty]
    private Scale? selectedScale;

    [ObservableProperty]
    private int currentIndex = 0;

    [ObservableProperty]
    private int totalQuestions = 0;

    [ObservableProperty]
    private int selectedOptionIndex = -1; // -1 表示未选择

    [ObservableProperty]
    private bool isCompleted = false;

    [ObservableProperty]
    private AssessmentResult result = new();

    // AI 个性化说明
    [ObservableProperty]
    private string aiExplanation = string.Empty;

    [ObservableProperty]
    private bool isExplaining = false;

    public bool HasAiExplanation => !string.IsNullOrWhiteSpace(AiExplanation);

    // 保存每道题选择的选项下标
    private readonly Dictionary<string, int> _answers = [];

    private void LoadBuiltInScales()
    {
        // 示例：PHQ-9 抑郁量表
        var phq9 = new Scale
        {
            Id = "phq9",
            Name = "PHQ-9 抑郁量表",
            Description = "过去两周内，以下问题困扰你的频率？",
            Questions = [],
        };

        string[] qs =
        [
            "对做事情提不起兴趣或乐趣",
            "感到心情低落、沮丧或绝望",
            "入睡困难、睡不安或睡眠过多",
            "感到疲倦或没有活力",
            "食欲不振或吃得过多",
            "觉得自己很差，或让自己或家人失望",
            "注意力不集中，如看电视或读书时",
            "动作或说话变慢，或坐立不安",
            "有不如死掉或用某种方式伤害自己的念头",
        ];

        foreach (var (q, i) in qs.Select((q, i) => (q, i)))
        {
            phq9.Questions.Add(
                new ScaleQuestion
                {
                    Id = (i + 1).ToString(),
                    Text = q,
                    Options =
                    [
                        new() { Text = "完全没有", Score = 0 },
                        new() { Text = "好几天", Score = 1 },
                        new() { Text = "一半以上天数", Score = 2 },
                        new() { Text = "几乎每天", Score = 3 },
                    ],
                }
            );
        }

        phq9.Interpretations =
        [
            (0, 4, "最轻度或无抑郁", "保持良好作息和社交活动"),
            (5, 9, "轻度", "建议适度运动、睡眠 hygiene"),
            (10, 14, "中度", "可考虑寻求专业咨询"),
            (15, 19, "中重度", "建议尽快联系专业人士"),
            (20, 27, "重度", "强烈建议尽快就医"),
        ];

        Scales.Add(phq9);

        // 示例：GAD-7 焦虑量表
        var gad7 = new Scale
        {
            Id = "gad7",
            Name = "GAD-7 焦虑量表",
            Description = "过去两周内，以下问题困扰你的频率？",
            Questions = [],
        };

        string[] gadQs =
        [
            "感到紧张、焦虑或如坐针毡",
            "难以停止或控制担忧",
            "对各种事情过度担忧",
            "很难放松下来",
            "坐立不安，以至于难以静坐",
            "容易烦躁或易怒",
            "担心将会发生可怕的事情",
        ];

        foreach (var (q, i) in gadQs.Select((q, i) => (q, i)))
        {
            gad7.Questions.Add(
                new ScaleQuestion
                {
                    Id = (i + 1).ToString(),
                    Text = q,
                    Options =
                    [
                        new() { Text = "完全没有", Score = 0 },
                        new() { Text = "几天", Score = 1 },
                        new() { Text = "一半以上天数", Score = 2 },
                        new() { Text = "几乎每天", Score = 3 },
                    ],
                }
            );
        }

        gad7.Interpretations =
        [
            (0, 4, "最轻度或无焦虑", "保持良好作息和社交活动"),
            (5, 9, "轻度", "建议进行放松训练与规律运动"),
            (10, 14, "中度", "可考虑寻求专业咨询"),
            (15, 21, "重度", "建议尽快联系专业人士"),
        ];

        Scales.Add(gad7);
        SelectedScale = phq9;
        TotalQuestions = phq9.Questions.Count;
        CurrentIndex = 0;
    }

    public ScaleQuestion? CurrentQuestion =>
        SelectedScale?.Questions.ElementAtOrDefault(CurrentIndex);

    // 1 基页码，便于 UI 显示 n / total 能到达最后一页
    public int CurrentPage => Math.Min(CurrentIndex + 1, TotalQuestions);

    // 是否首题/末题，便于 UI 控制按钮显示
    public bool IsFirstQuestion => CurrentIndex <= 0;
    public bool IsLastQuestion => TotalQuestions <= 0 || CurrentIndex >= TotalQuestions - 1;

    partial void OnCurrentIndexChanged(int value)
    {
        // 当切题时，恢复之前选择
        if (CurrentQuestion != null && _answers.TryGetValue(CurrentQuestion.Id, out var idx))
        {
            SelectedOptionIndex = idx;
        }
        else
        {
            SelectedOptionIndex = -1;
        }

        // 通知当前题目与页码变更，避免 UI 只显示第一题
        OnPropertyChanged(nameof(CurrentQuestion));
        OnPropertyChanged(nameof(CurrentPage));
        OnPropertyChanged(nameof(IsFirstQuestion));
        OnPropertyChanged(nameof(IsLastQuestion));

        // 刷新命令可用性
        PrevCommand.NotifyCanExecuteChanged();
        NextCommand.NotifyCanExecuteChanged();
        SubmitCommand.NotifyCanExecuteChanged();
        ExplainByAiCommand.NotifyCanExecuteChanged();
    }

    partial void OnSelectedOptionIndexChanged(int value)
    {
        // 实时保存当前题目的选择，确保最后一题选择后可立即提交
        if (CurrentQuestion != null && value >= 0)
        {
            _answers[CurrentQuestion.Id] = value;
        }
        NextCommand.NotifyCanExecuteChanged();
        SubmitCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanPrev))]
    private void Prev()
    {
        if (CurrentIndex > 0)
            CurrentIndex--;
    }

    private bool CanPrev() => CurrentIndex > 0;

    [RelayCommand(CanExecute = nameof(CanNext))]
    private void Next()
    {
        if (CurrentQuestion is null)
            return;
        if (SelectedOptionIndex >= 0)
        {
            _answers[CurrentQuestion.Id] = SelectedOptionIndex;
        }
        if (CurrentIndex < TotalQuestions - 1)
            CurrentIndex++;
    }

    private bool CanNext() => SelectedOptionIndex >= 0 && !IsLastQuestion;

    [RelayCommand(CanExecute = nameof(CanSubmit))]
    private void Submit()
    {
        if (SelectedScale is null)
            return;
        // 保存最后一题
        if (CurrentQuestion is not null && SelectedOptionIndex >= 0)
        {
            _answers[CurrentQuestion.Id] = SelectedOptionIndex;
        }

        int total = 0;
        foreach (var q in SelectedScale.Questions)
        {
            if (_answers.TryGetValue(q.Id, out var i))
            {
                total += q.Options[i].Score;
            }
        }

        var match = SelectedScale.Interpretations.FirstOrDefault(it =>
            total >= it.Min && total <= it.Max
        );
        // 兜底：若未匹配到任何区间，则按接近规则给出提示
        var level = string.IsNullOrWhiteSpace(match.Level) ? "未分级" : match.Level;
        var advice = string.IsNullOrWhiteSpace(match.Advice)
            ? "建议：请核对各题作答是否完整，或联系开发者补全解释区间。"
            : match.Advice;

        Result = new AssessmentResult
        {
            ScaleId = SelectedScale.Id,
            ScaleName = SelectedScale.Name,
            TotalScore = total,
            Level = level,
            Advice = advice,
        };
        IsCompleted = true;
    }

    private bool CanSubmit() =>
        SelectedScale is not null && _answers.Count == SelectedScale.Questions.Count;

    partial void OnSelectedScaleChanged(Scale? value)
    {
        // 切换量表时重置进度与答案
        _answers.Clear();
        AiExplanation = string.Empty;
        IsExplaining = false;
        IsCompleted = false;
        Result = new AssessmentResult();
        CurrentIndex = 0;
        TotalQuestions = value?.Questions.Count ?? 0;
        SelectedOptionIndex = -1;

        // 通知与当前题目/页码相关的属性，确保首题能正确显示
        OnPropertyChanged(nameof(CurrentQuestion));
        OnPropertyChanged(nameof(CurrentPage));
        OnPropertyChanged(nameof(IsFirstQuestion));
        OnPropertyChanged(nameof(IsLastQuestion));

        PrevCommand.NotifyCanExecuteChanged();
        NextCommand.NotifyCanExecuteChanged();
        SubmitCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private void Restart()
    {
        OnSelectedScaleChanged(SelectedScale);
    }

    [RelayCommand(CanExecute = nameof(CanExplain))]
    private async Task ExplainByAiAsync()
    {
        // 额外防护，避免极端并发触发
        if (!CanExplain())
            return;
        var scale = SelectedScale;
        var result = Result;
        if (scale is null || result is null)
            return;

        IsExplaining = true;
        AiExplanation = string.Empty;

        // 根据答案拼装简要描述
        var answerSummary = BuildAnswerSummary(scale);
        var prompt =
            $"你是一名心理咨询师。基于以下量表结果提供温和、务实、非医疗诊断的解释与建议（300字内，中文）：{Environment.NewLine}"
            + $"量表：{result.ScaleName}{Environment.NewLine}"
            + $"总分：{result.TotalScore}（{result.Level}）{Environment.NewLine}"
            + $"答案概览：{Environment.NewLine}{answerSummary}";

        try
        {
            await _apiService.ChatAsync(
                prompt,
                async (t, s) =>
                {
                    if (t == ResponseType.Content)
                    {
                        AiExplanation += s;
                    }
                    await Task.CompletedTask;
                },
                historyMessages: null
            );
        }
        catch (Exception)
        {
            AiExplanation = "生成解释失败，请稍后重试。";
        }
        finally
        {
            IsExplaining = false;
        }
    }

    private string BuildAnswerSummary(Scale scale)
    {
        return string.Join(
            Environment.NewLine,
            scale.Questions.Select(q =>
            {
                var picked = _answers.TryGetValue(q.Id, out var idx)
                    ? q.Options[idx].Text
                    : "未作答";
                return $"- {q.Text}：{picked}";
            })
        );
    }

    private bool CanExplain() =>
        IsCompleted && !IsExplaining && SelectedScale is not null && Result is not null;

    partial void OnAiExplanationChanged(string value)
    {
        OnPropertyChanged(nameof(HasAiExplanation));
    }

    partial void OnIsCompletedChanged(bool value)
    {
        ExplainByAiCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsExplainingChanged(bool value)
    {
        ExplainByAiCommand.NotifyCanExecuteChanged();
    }
}
