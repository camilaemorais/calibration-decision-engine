using Microsoft.Extensions.Logging;

namespace CalibrationDecisionEngine.Pipeline;

public sealed class PipelineBuilder<TIn, TOut>
{
    private readonly List<StepInvoker> _steps;
    private readonly ILoggerFactory? _loggerFactory;

    public PipelineBuilder(ILoggerFactory? loggerFactory = null)
        : this(new List<StepInvoker>(), loggerFactory)
    {
    }

    private PipelineBuilder(List<StepInvoker> steps, ILoggerFactory? loggerFactory)
    {
        _steps = steps;
        _loggerFactory = loggerFactory;
    }

    public PipelineBuilder<TIn, TNext> AddStep<TNext>(IPipelineStep<TOut, TNext> step)
    {
        ArgumentNullException.ThrowIfNull(step);
        var newSteps = new List<StepInvoker>(_steps) { new StepInvoker<TOut, TNext>(step) };
        return new PipelineBuilder<TIn, TNext>(newSteps, _loggerFactory);
    }

    public IPipeline<TIn, TOut> Build()
    {
        if (_steps.Count == 0)
            throw new InvalidOperationException("Pipeline must contain at least one step.");

        var logger = _loggerFactory?.CreateLogger<Pipeline<TIn, TOut>>();
        return new Pipeline<TIn, TOut>(_steps, logger);
    }
}
