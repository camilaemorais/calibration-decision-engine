namespace CalibrationDecisionEngine.Domain.Models;

public sealed class MatchedCalibration
{
    public string RuleName { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string Trigger { get; init; } = string.Empty;
}
