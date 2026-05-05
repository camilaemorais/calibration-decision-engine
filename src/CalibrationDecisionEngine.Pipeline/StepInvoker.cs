namespace CalibrationDecisionEngine.Pipeline;

// The Pipeline stores steps with potentially different TIn/TOut pairs (heterogeneous chain).
// We can't put IPipelineStep<,> directly in a single typed list, so we wrap each one in a
// non-generic invoker that boxes the input to object and unboxes inside.
//
// This is the only piece of "untyped" code in the framework — public APIs stay strongly typed.
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
