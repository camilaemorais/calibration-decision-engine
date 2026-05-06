namespace CalibrationDecisionEngine.Pipeline;

public interface IPipelineStep<TIn, TOut>
{
    Task<TOut> ExecuteAsync(TIn input, CancellationToken cancellationToken);
}
