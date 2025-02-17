using System;
using System.Collections.Generic;

namespace AvaMujica.Models;

public class HistoryManager { }

public class HistoryInfo
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

public class SQLiteHistory
{
    public List<HistoryInfo> History { get; set; } = new List<HistoryInfo>();
}
