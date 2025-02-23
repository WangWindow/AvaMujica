using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Avalonia.Media;

namespace AvaMujica.Converters;

public class BooleanToHorizontalAlignmentConverter : IValueConverter
{
    public static readonly BooleanToHorizontalAlignmentConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool isFromUser && isFromUser
            ? HorizontalAlignment.Right
            : HorizontalAlignment.Left;
    }

    public object? ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    )
    {
        throw new NotImplementedException();
    }
}

public class MessageColorConverter : IValueConverter
{
    public static readonly MessageColorConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool isFromUser
            ? new SolidColorBrush(Color.Parse("#E3F2FD")) // 用户消息使用浅蓝色背景
            : new SolidColorBrush(Color.Parse("#F5F5F5")); // 机器人消息使用浅灰色背景
    }

    public object? ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    )
    {
        throw new NotImplementedException();
    }
}
