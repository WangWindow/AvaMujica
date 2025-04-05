using System;
using System.Globalization;
using Avalonia.Data.Converters;
using AvaMujica.Models;

namespace AvaMujica;

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
    /// 反向转换，用于RadioButton选择等双向绑定场景
    /// </summary>
    public object? ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    )
    {
        if (value is bool boolValue && boolValue)
        {
            return parameter;
        }
        return null;
    }
}

/// <summary>
/// 将API Key转换为掩码显示的转换器
/// </summary>
public class ApiKeyMaskConverter : IValueConverter
{
    /// <summary>
    /// 将API Key转换为掩码形式显示
    /// </summary>
    /// <param name="value">API Key值</param>
    /// <param name="targetType">目标类型</param>
    /// <param name="parameter">参数</param>
    /// <param name="culture">文化信息</param>
    /// <returns>掩码后的API Key</returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                return "未设置";
            }

            // 只显示前4位和后4位，中间用星号代替
            if (apiKey.Length <= 8)
            {
                return new string('*', apiKey.Length);
            }

            return $"{apiKey[..4]}****{apiKey[^4..]}";
        }

        return "未设置";
    }

    /// <summary>
    /// 反向转换，这里不需要实现
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
