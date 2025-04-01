using System;
using System.Globalization;
using Avalonia.Data.Converters;
using AvaMujica.Models;

namespace AvaMujica;

/// <summary>
/// 提供XAML中使用的值转换器
/// </summary>
public class RoleToIsUserConverter : IValueConverter
{
    /// <summary>
    /// 将Role值转换为是否是用户消息的布尔值
    /// </summary>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string role)
        {
            bool isUser = role == "user";

            // 如果提供了参数，且参数为字符串"false"，则反转结果
            if (parameter is string param && param.ToLower() == "false")
            {
                return !isUser;
            }

            return isUser;
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
