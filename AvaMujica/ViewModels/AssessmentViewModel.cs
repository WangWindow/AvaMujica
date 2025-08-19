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

public partial class AssessmentViewModel : ViewModelBase
{
    private readonly IApiService _apiService;

    public ObservableCollection<Scale> Scales { get; } = new();

    [ObservableProperty]
    private Scale? selectedScale;

    [ObservableProperty]
    private int currentIndex = 0;

    [ObservableProperty]
    private int totalQuestions = 0;

    [ObservableProperty]
    private int? selectedOptionIndex;

    [ObservableProperty]
    private bool isCompleted = false;

    [ObservableProperty]
    private AssessmentResult? result;

    // 保存每道题选择的选项下标
    private readonly Dictionary<string, int> _answers = new();

    public AssessmentViewModel(IApiService apiService)
    {
        _apiService = apiService;
        LoadBuiltInScales();
    }

    private void LoadBuiltInScales()
    {
        // 示例：PHQ-9 抑郁量表
        var phq9 = new Scale
        {
            Id = "phq9",
            Name = "PHQ-9 抑郁量表",
            Description = "过去两周内，以下问题困扰你的频率？",
            Questions = new List<ScaleQuestion>()
        };

        string[] qs = new string[]
        {
            "对做事情提不起兴趣或乐趣",
            "感到心情低落、沮丧或绝望",
            "入睡困难、睡不安或睡眠过多",
            "感到疲倦或没有活力",
            "食欲不振或吃得过多",
            "觉得自己很差，或让自己或家人失望",
            "注意力不集中，如看电视或读书时",
            "动作或说话变慢，或坐立不安",
            "有不如死掉或用某种方式伤害自己的念头"
        };

        foreach (var (q, i) in qs.Select((q, i) => (q, i)))
        {
            phq9.Questions.Add(new ScaleQuestion
            {
                Id = (i + 1).ToString(),
                Text = q,
                Options = new List<ScaleOption>
                {
                    new() { Text = "完全没有", Score = 0 },
                    new() { Text = "好几天", Score = 1 },
                    new() { Text = "一半以上天数", Score = 2 },
                    new() { Text = "几乎每天", Score = 3 }
                }
            });
        }

        phq9.Interpretations = new()
        {
            (0,4,"最轻度或无抑郁","保持良好作息和社交活动"),
            (5,9,"轻度","建议适度运动、睡眠 hygiene"),
            (10,14,"中度","可考虑寻求专业咨询"),
            (15,19,"中重度","建议尽快联系专业人士"),
            (20,27,"重度","强烈建议尽快就医")
        };

        Scales.Add(phq9);
        SelectedScale = phq9;
        TotalQuestions = phq9.Questions.Count;
        CurrentIndex = 0;
    }

    public ScaleQuestion? CurrentQuestion => SelectedScale?.Questions.ElementAtOrDefault(CurrentIndex);

    partial void OnCurrentIndexChanged(int value)
    {
        // 当切题时，恢复之前选择
        if (CurrentQuestion != null && _answers.TryGetValue(CurrentQuestion.Id, out var idx))
        {
            SelectedOptionIndex = idx;
        }
        else
        {
            SelectedOptionIndex = null;
        }
    }

    [RelayCommand(CanExecute = nameof(CanPrev))]
    private void Prev()
    {
        if (CurrentIndex > 0) CurrentIndex--;
    }

    private bool CanPrev() => CurrentIndex > 0;

    [RelayCommand(CanExecute = nameof(CanNext))]
    private void Next()
    {
        if (CurrentQuestion is null) return;
        if (SelectedOptionIndex is int idx)
        {
            _answers[CurrentQuestion.Id] = idx;
        }
        if (CurrentIndex < TotalQuestions - 1) CurrentIndex++;
    }

    private bool CanNext() => SelectedOptionIndex is int;

    [RelayCommand(CanExecute = nameof(CanSubmit))]
    private void Submit()
    {
        if (SelectedScale is null) return;
        // 保存最后一题
        if (CurrentQuestion is not null && SelectedOptionIndex is int idx)
        {
            _answers[CurrentQuestion.Id] = idx;
        }

        int total = 0;
        foreach (var q in SelectedScale.Questions)
        {
            if (_answers.TryGetValue(q.Id, out var i))
            {
                total += q.Options[i].Score;
            }
        }

        var interp = SelectedScale.Interpretations.FirstOrDefault(it => total >= it.Min && total <= it.Max);
        Result = new AssessmentResult
        {
            ScaleId = SelectedScale.Id,
            ScaleName = SelectedScale.Name,
            TotalScore = total,
            Level = interp.Level,
            Advice = interp.Advice
        };
        IsCompleted = true;
    }

    private bool CanSubmit() => SelectedScale is not null && _answers.Count == SelectedScale.Questions.Count;
}
