namespace CalibrationDecisionEngine.Domain.Models;

public sealed class CalibrationRule
{
    public string Name { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public IReadOnlyList<string> MatchKeywords { get; init; } = [];
    public IReadOnlyList<string> Excludes { get; init; } = [];
}
