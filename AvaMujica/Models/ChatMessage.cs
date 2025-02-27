/*
 * @FilePath: ChatMessage.cs
 * @Author: WangWindow 1598593280@qq.com
 * @Date: 2025-02-22 16:25:34
 * @LastEditors: WangWindow
 * @LastEditTime: 2025-02-25 21:52:54
 * 2025 by WangWindow, All Rights Reserved.
 * @Description:
 */
using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaMujica.Models;

/// <summary>
/// 聊天消息类
/// </summary>
public partial class ChatMessage : ObservableObject
{
    /// <summary>
    /// 消息内容
    /// </summary>
    [ObservableProperty]
    private string _content = string.Empty;

    /// <summary>
    /// 推理过程
    /// </summary>
    [ObservableProperty]
    private string _reasoningContent = string.Empty;

    /// <summary>
    /// 消息发送时间
    /// </summary>
    public DateTime Time { get; set; }

    /// <summary>
    /// 消息是否来自用户
    /// </summary>
    public bool IsFromUser { get; set; }
}
