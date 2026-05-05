namespace CalibrationDecisionEngine.Pipeline;

/// <summary>
/// A single, composable unit of work in a pipeline.
/// Steps must be stateless across executions: any per-execution state belongs on the input/output context.
/// </summary>
public interface IPipelineStep<TIn, TOut>
{
    Task<TOut> ExecuteAsync(TIn input, CancellationToken cancellationToken);
}
