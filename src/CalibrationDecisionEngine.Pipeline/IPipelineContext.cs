namespace CalibrationDecisionEngine.Pipeline;

/// <summary>
/// Optional contract a context can implement to communicate human-readable notes
/// to the pipeline runner after each step finishes. Notes show up in the trace.
/// </summary>
/// <remarks>
/// We don't constrain the generic <see cref="IPipeline{TIn,TOut}"/> on this interface
/// because intermediate types in a heterogeneous pipeline may not be contexts.
/// The runner does an <c>is</c>-check at runtime instead.
/// </remarks>
public interface IPipelineContext
{
    string? StepNotes { get; set; }
}
