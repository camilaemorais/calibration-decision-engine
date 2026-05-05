using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace CalibrationDecisionEngine.Pipeline;

internal sealed class Pipeline<TIn, TOut> : IPipeline<TIn, TOut>
{
    private readonly IReadOnlyList<StepInvoker> _steps;
    private readonly ILogger<Pipeline<TIn, TOut>>? _logger;

    public Pipeline(IReadOnlyList<StepInvoker> steps, ILogger<Pipeline<TIn, TOut>>? logger)
    {
        _steps = steps;
        _logger = logger;
    }

    public async Task<PipelineResult<TOut>> ExecuteAsync(TIn input, CancellationToken cancellationToken = default)
    {
        var trace = new List<PipelineTraceEntry>(_steps.Count);
        object current = input!;

        foreach (var step in _steps)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var scope = _logger?.BeginScope("Step:{StepName}", step.Name);
            var sw = Stopwatch.StartNew();

            try
            {
                current = await step.InvokeAsync(current, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                sw.Stop();
                trace.Add(new PipelineTraceEntry(step.Name, sw.ElapsedMilliseconds, "cancelled"));
                _logger?.LogInformation("Pipeline cancelled at step {StepName} after {DurationMs}ms",
                    step.Name, sw.ElapsedMilliseconds);
                throw;
            }
            catch (Exception ex)
            {
                sw.Stop();
                trace.Add(new PipelineTraceEntry(step.Name, sw.ElapsedMilliseconds, $"failed: {ex.GetType().Name}"));
                _logger?.LogError(ex, "Pipeline step {StepName} failed after {DurationMs}ms",
                    step.Name, sw.ElapsedMilliseconds);
                throw new PipelineExecutionException(step.Name, ex, trace);
            }

            sw.Stop();

            // Pull notes off the context (if any) so the runner can attach them to the trace
            // without coupling steps to a logging API.
            string? notes = null;
            if (current is IPipelineContext ctx)
            {
                notes = ctx.StepNotes;
                ctx.StepNotes = null;
            }

            trace.Add(new PipelineTraceEntry(step.Name, sw.ElapsedMilliseconds, notes));
            _logger?.LogDebug("Step {StepName} completed in {DurationMs}ms ({Notes})",
                step.Name, sw.ElapsedMilliseconds, notes);
        }

        return new PipelineResult<TOut>((TOut)current, trace);
    }
}
