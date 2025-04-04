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
