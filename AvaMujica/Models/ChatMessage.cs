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
    private string content = string.Empty;

    /// <summary>
    /// 推理内容
    /// </summary>
    [ObservableProperty]
    private string reasoningContent = string.Empty;

    /// <summary>
    /// 消息时间
    /// </summary>
    [ObservableProperty]
    private DateTime time;

    /// <summary>
    /// 是否是用户发送的消息
    /// </summary>
    [ObservableProperty]
    private bool isFromUser;

    /// <summary>
    /// 是否正在加载中
    /// </summary>
    [ObservableProperty]
    private bool isLoading;
}
