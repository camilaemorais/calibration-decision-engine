namespace CalibrationDecisionEngine.Pipeline;

public sealed class PipelineResult<TOut>
{
    public TOut Output { get; }
    public IReadOnlyList<PipelineTraceEntry> Trace { get; }

    public PipelineResult(TOut output, IReadOnlyList<PipelineTraceEntry> trace)
    {
        Output = output;
        Trace = trace;
    }
}
