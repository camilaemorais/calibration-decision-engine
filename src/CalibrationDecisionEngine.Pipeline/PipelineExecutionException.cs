namespace CalibrationDecisionEngine.Pipeline;

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
