using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;

namespace AvaMujica.Views;

public partial class PlanView : UserControl
{
    private const double NarrowThreshold = 520.0; // 根据控件总宽度经验阈值
    private Grid? _root;
    private Grid? _wideBar;
    private Grid? _narrowBar;
    private StackPanel? _wideButtons;

    public PlanView()
    {
        InitializeComponent();
        this.AttachedToVisualTree += OnAttachedToVisualTree;
        this.DetachedFromVisualTree += OnDetachedFromVisualTree;
    }

    // 使用 SizeChanged 事件，无需 VM 状态或 Reactive 依赖

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        // 获取命名控件
        _root = this.FindControl<Grid>("Root");
        _wideBar = this.FindControl<Grid>("WideBar");
        _narrowBar = this.FindControl<Grid>("NarrowBar");
        _wideButtons = this.FindControl<StackPanel>("WideButtons");

        // 初始计算
        UpdateLayoutMode(Bounds.Width);
        // 订阅尺寸变化
        this.SizeChanged += OnSizeChanged;
    }

    private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        this.SizeChanged -= OnSizeChanged;
    }

    private void UpdateLayoutMode(double width)
    {
        var requiredForWide = EstimateRequiredWidthForWide();
        var narrow = width < requiredForWide;
        _wideBar?.IsVisible = !narrow;
        _narrowBar?.IsVisible = narrow;
    }

    private double EstimateRequiredWidthForWide()
    {
        // 基于按钮实际宽度 + 输入框最小宽度 + 余量 估算阈值
        double buttons = _wideButtons?.Bounds.Width ?? 0;
        if (double.IsNaN(buttons) || buttons <= 0)
        {
            // 尝试用 DesiredSize 估计
            try
            {
                _wideButtons?.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                buttons = _wideButtons?.DesiredSize.Width ?? 0;
            }
            catch
            {
                buttons = 300; // 兜底估值
            }
        }

        const double inputMin = 240; // 输入框最小可用宽度
        const double margins = 40;    // Root 左右 Margin 20+20
        const double gap = 8;         // 输入与按钮间距
        var threshold = buttons + inputMin + margins + gap;

        // 与固定下限取大，避免抖动
        return Math.Max(threshold, NarrowThreshold);
    }

    private void OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        UpdateLayoutMode(e.NewSize.Width);
    }
}
