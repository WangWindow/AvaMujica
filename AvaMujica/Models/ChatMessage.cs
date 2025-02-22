using System;

namespace AvaMujica.Models;

public class ChatMessage
{
    public string Content { get; set; } = string.Empty;
    public bool IsFromUser { get; set; }
    public DateTime Time { get; set; }
}
