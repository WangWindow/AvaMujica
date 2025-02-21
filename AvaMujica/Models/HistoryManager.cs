using System;
using System.Collections.Generic;

namespace AvaMujica.Models;

public class HistoryManager { }

public abstract class HistoryInfo
{
    public int Id { get; set; }
    public string User { get; set; } = "User Name";
    public string Action { get; set; } = "No Action";
    public DateTime Date { get; set; }
}

public class JsonHistory
{
    public List<HistoryInfo> History { get; set; } = new List<HistoryInfo>();
}

public class SqLiteHistory
{
    public List<HistoryInfo> History { get; set; } = new List<HistoryInfo>();
}
