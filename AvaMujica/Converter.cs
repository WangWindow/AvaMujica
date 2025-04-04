using System;
using System.Globalization;
using Avalonia.Data.Converters;
using AvaMujica.Models;

namespace AvaMujica;

/// <summary>
/// 提供消息加载状态的值转换器
/// </summary>
public class MessageLoadingConverter : IValueConverter
{
    /// <summary>
    /// 根据消息内容和角色判断是否处于加载状态
    /// </summary>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ChatMessage message)
        {
            return string.IsNullOrEmpty(message.Content) && message.Role == "assistant";
        }
        return false;
    }

    /// <summary>
    /// 反向转换（不实现）
    /// </summary>
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

/// <summary>
/// 通用值比较转换器，比较输入值是否与参数相等
/// </summary>
public class ValueEqualsConverter : IValueConverter
{
    /// <summary>
    /// 将输入值与参数进行比较，相等则返回true，否则返回false
    /// </summary>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value?.Equals(parameter);
    }

    /// <summary>
    /// 反向转换（不实现）
    /// </summary>
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
