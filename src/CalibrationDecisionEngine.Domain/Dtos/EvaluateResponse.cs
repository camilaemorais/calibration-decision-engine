namespace CalibrationDecisionEngine.Domain.Dtos;

public sealed class EvaluateResponse
{
    public string Vin { get; init; } = string.Empty;
    public IReadOnlyList<CalibrationResultDto> RequiredCalibrations { get; init; } = [];
    public IReadOnlyList<TraceEntryDto> Trace { get; init; } = [];
}

public sealed class CalibrationResultDto
{
    public string RuleName { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string Trigger { get; init; } = string.Empty;
}

public sealed class TraceEntryDto
{
    public string Step { get; init; } = string.Empty;
    public long DurationMs { get; init; }
    public string? Notes { get; init; }
}
