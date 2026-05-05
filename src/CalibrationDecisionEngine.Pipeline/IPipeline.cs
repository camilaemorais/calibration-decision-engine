namespace CalibrationDecisionEngine.Pipeline;

public interface IPipeline<TIn, TOut>
{
    Task<PipelineResult<TOut>> ExecuteAsync(TIn input, CancellationToken cancellationToken = default);
}
