using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaMujica.ViewModels;

namespace AvaMujica.Views;

/// <summary>
/// SettingsView.xaml 的交互逻辑
/// </summary>
public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <summary>
    /// 模型选择下拉框的选择变化事件处理
    /// </summary>
    private void ComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (
            DataContext is SettingsViewModel viewModel
            && sender is ComboBox comboBox
            && comboBox.SelectedItem is string selectedModel
        )
        {
            // 执行选择模型的命令
            viewModel.SelectModelCommand.Execute(selectedModel);
        }
    }
}
