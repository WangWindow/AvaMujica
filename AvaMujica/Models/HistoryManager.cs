/*
 * @FilePath: HistoryManager.cs
 * @Author: WangWindow 1598593280@qq.com
 * @Date: 2025-02-21 16:27:39
 * @LastEditors: WangWindow
 * @LastEditTime: 2025-02-25 21:55:21
 * 2025 by WangWindow, All Rights Reserved.
 * @Description:
 */
using System;
using System.Collections.Generic;

namespace AvaMujica.Models;

/// <summary>
/// 历史记录管理器
/// </summary>
public class HistoryManager { }

/// <summary>
/// 历史记录信息
/// </summary>
public class HistoryInfo
{
    /// <summary>
    /// 历史记录 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 历史记录标题
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 历史记录时间
    /// </summary>
    public DateTime Time { get; set; }

    /// <summary>
    /// 格式化后的时间显示
    /// </summary>
    public string FormattedTime
    {
        get
        {
            if (Time.Date == DateTime.Today)
            {
                return Time.ToString("HH:mm");
            }
            else
            {
                return Time.ToString("MM-dd");
            }
        }
    }
}

/// <summary>
/// Json 格式的历史记录处理
/// </summary>
public class JsonHistory
{
    public List<HistoryInfo> History { get; set; } = [];
}

/// <summary>
/// Sqlite 数据库的历史记录处理
/// </summary>
public class SqLiteHistory
{
    public List<HistoryInfo> History { get; set; } = [];
}
