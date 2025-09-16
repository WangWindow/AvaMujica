using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaMujica.Models;

/// <summary>
/// 会话类
/// </summary>
public partial class ChatSession : ObservableObject
{
    /// <summary>
    /// 会话ID
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// 会话标题
    /// </summary>
    public string Title { get; set; } = "新会话";

    /// <summary>
    /// 会话类型
    /// </summary>
    public string Type { get; set; } = SessionType.Chat;

    /// <summary>
    /// 会话创建时间
    /// </summary>
    public DateTime CreatedTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 会话更新时间
    /// </summary>
    [ObservableProperty]
    private DateTime updatedTime = DateTime.Now;

    /// <summary>
    /// 仅用于显示的“更新日期”文本：
    /// - 若为今年：显示 MM-dd
    /// - 否则：显示 yyyy-MM-dd
    /// </summary>
    public string UpdatedDateDisplay
    {
        get
        {
            var now = DateTime.Now;
            if (UpdatedTime.Year == now.Year)
            {
                return UpdatedTime.ToString("MM-dd");
            }
            return UpdatedTime.ToString("yyyy-MM-dd");
        }
    }

    /// <summary>
    /// 会话消息列表
    /// </summary>
    public List<ChatMessage> Messages { get; set; } = [];
}
