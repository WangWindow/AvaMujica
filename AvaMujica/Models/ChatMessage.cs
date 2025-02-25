using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaMujica.Models;

public partial class ChatMessage : ObservableObject
{
    [ObservableProperty]
    private string _content = string.Empty;

    public DateTime Time { get; set; }
    public bool IsFromUser { get; set; }
}
