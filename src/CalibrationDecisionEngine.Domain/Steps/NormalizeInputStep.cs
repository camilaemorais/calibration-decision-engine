using CalibrationDecisionEngine.Domain.Models;
using CalibrationDecisionEngine.Pipeline;

namespace CalibrationDecisionEngine.Domain.Steps;

public sealed class NormalizeInputStep : IPipelineStep<VehicleContext, VehicleContext>
{
    public Task<VehicleContext> ExecuteAsync(VehicleContext input, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        foreach (var line in input.Lines)
        {
            line.Description = line.Description.Trim();
            line.Operation = line.Operation.Trim().ToUpperInvariant();
        }

        input.Lines = input.Lines
            .DistinctBy(l => (l.Description.ToUpperInvariant(), l.Operation))
            .ToList();

        input.StepNotes = $"{input.Lines.Count} lines normalized";
        return Task.FromResult(input);
    }
}
