/*
 * @FilePath: HistoryManager.cs
 * @Author: WangWindow 1598593280@qq.com
 * @Date: 2025-02-21 16:27:39
 * @LastEditors: WangWindow
 * @LastEditTime: 2025-02-28 00:41:58
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
