using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace AvaMujica.Views
{
    public partial class ChatView : UserControl
    {
        private ScrollViewer? _messageScroller;
        private ViewModels.ChatViewModel? _viewModel;

        public ChatView()
        {
            InitializeComponent();
            Loaded += OnChatViewLoaded;
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object? sender, EventArgs e)
        {
            _viewModel = DataContext as ViewModels.ChatViewModel;
        }

        private void OnChatViewLoaded(object? sender, RoutedEventArgs e)
        {
            _messageScroller = this.FindControl<ScrollViewer>("MessageScroller");
            _viewModel = DataContext as ViewModels.ChatViewModel;

            // 滚动到底部
            ScrollToBottom();

            // 订阅 ViewModel 的滚动事件
            if (_viewModel != null)
            {
                _viewModel.ScrollToBottomRequested += ScrollToBottom;
            }
        }

        private void ScrollToBottom()
        {
            // 确保消息列表滚动到底部
            if (_messageScroller != null)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    _messageScroller.ScrollToEnd();
                });
            }
        }
    }
}
