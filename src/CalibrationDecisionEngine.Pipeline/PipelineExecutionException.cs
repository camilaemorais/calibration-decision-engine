namespace CalibrationDecisionEngine.Pipeline;

/// <summary>
/// Wraps an exception thrown inside a step so callers can see <em>which</em> step failed
/// and the partial trace up to and including the failure.
/// </summary>
public sealed class PipelineExecutionException : Exception
{
    public string FailedStep { get; }
    public IReadOnlyList<PipelineTraceEntry> PartialTrace { get; }

    public PipelineExecutionException(string failedStep, Exception inner, IReadOnlyList<PipelineTraceEntry> partialTrace)
        : base($"Pipeline step '{failedStep}' failed: {inner.Message}", inner)
    {
        FailedStep = failedStep;
        PartialTrace = partialTrace;
    }
}
