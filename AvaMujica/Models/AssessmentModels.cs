using System.Collections.Generic;

namespace AvaMujica.Models;

public class ScaleOption
{
    public string Text { get; set; } = string.Empty;
    public int Score { get; set; }
}

public class ScaleQuestion
{
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public List<ScaleOption> Options { get; set; } = [];
}

public class Scale
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<ScaleQuestion> Questions { get; set; } = [];
    public List<(int Min, int Max, string Level, string Advice)> Interpretations { get; set; } = [];
}

public class AssessmentResult
{
    public string ScaleId { get; set; } = string.Empty;
    public string ScaleName { get; set; } = string.Empty;
    public int TotalScore { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Advice { get; set; } = string.Empty;
}
