namespace CalibrationDecisionEngine.Pipeline;

public sealed record PipelineTraceEntry(string Step, long DurationMs, string? Notes);
