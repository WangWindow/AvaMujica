using System;
using System.Collections.ObjectModel;

namespace AvaMujica.Models;

public class PlanItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string? Note { get; set; }
    public bool IsDone { get; set; }
    public DateTime? DueDate { get; set; }
}

public class Plan
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "我的干预方案";
    public ObservableCollection<PlanItem> Items { get; set; } = [];
}
