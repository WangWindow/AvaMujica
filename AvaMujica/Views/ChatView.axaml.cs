using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace AvaMujica.Views;

public partial class ChatView : UserControl
{
    /// <summary>
    /// 消息滚动视图
    /// </summary>
    private ScrollViewer? _messageScroller;

    /// <summary>
    /// 聊天视图模型
    /// </summary>
    private ViewModels.ChatViewModel? _viewModel;

    /// <summary>
    /// 聊天视图
    /// </summary>
    public ChatView()
    {
        InitializeComponent();
        Loaded += OnChatViewLoaded;
        DataContextChanged += OnDataContextChanged;
    }

    /// <summary>
    /// 处理数据上下文变化事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        _viewModel = DataContext as ViewModels.ChatViewModel;
    }

    /// <summary>
    /// 处理聊天视图加载事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnChatViewLoaded(object? sender, RoutedEventArgs e)
    {
        _messageScroller = this.FindControl<ScrollViewer>("MessageScroller");
        _viewModel = DataContext as ViewModels.ChatViewModel;
    }
}
