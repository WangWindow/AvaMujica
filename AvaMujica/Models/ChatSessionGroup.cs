using System;
using System.Collections.Generic;

namespace AvaMujica.Models;

/// <summary>
/// 会话分组类
/// </summary>
public class ChatSessionGroup(string key, List<ChatSession> items)
{
    /// <summary>
    /// 分组的键
    /// </summary>
    public string Key { get; } = key;

    /// <summary>
    /// 分组的历史记录信息
    /// </summary>
    public List<ChatSession> Items { get; } = items;
}
