namespace CalibrationDecisionEngine.Pipeline;

internal abstract class StepInvoker
{
    public abstract string Name { get; }
    public abstract Task<object> InvokeAsync(object input, CancellationToken cancellationToken);
}

internal sealed class StepInvoker<TIn, TOut> : StepInvoker
{
    private readonly IPipelineStep<TIn, TOut> _step;

    public StepInvoker(IPipelineStep<TIn, TOut> step)
    {
        _step = step ?? throw new ArgumentNullException(nameof(step));
    }

    public override string Name => _step.GetType().Name;

    public override async Task<object> InvokeAsync(object input, CancellationToken cancellationToken)
    {
        var typed = (TIn)input;
        var result = await _step.ExecuteAsync(typed, cancellationToken).ConfigureAwait(false);
        return result!;
    }
}
