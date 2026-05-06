using CalibrationDecisionEngine.Domain.Models;
using CalibrationDecisionEngine.Pipeline;

namespace CalibrationDecisionEngine.Domain.Steps;

public sealed class ApplyExclusionsStep : IPipelineStep<VehicleContext, VehicleContext>
{
    private readonly IReadOnlyList<CalibrationRule> _rules;

    public ApplyExclusionsStep(IReadOnlyList<CalibrationRule> rules)
    {
        _rules = rules;
    }

    public Task<VehicleContext> ExecuteAsync(VehicleContext input, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var calibrations = input.MatchedCalibrations.ToList();
        var before = calibrations.Count;

        var hasAirbag = input.Lines.Any(l =>
            l.Description.Contains("airbag", StringComparison.OrdinalIgnoreCase));

        if (!hasAirbag)
            calibrations = calibrations.Where(c => c.RuleName != "Bumper Sensor Calibration").ToList();

        var survivingNames = calibrations.Select(c => c.RuleName).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var toExclude = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var rule in _rules)
        {
            if (!survivingNames.Contains(rule.Name)) continue;
            foreach (var excluded in rule.Excludes)
                toExclude.Add(excluded);
        }

        input.FinalCalibrations = calibrations
            .Where(c => !toExclude.Contains(c.RuleName))
            .ToList();

        input.StepNotes = $"{before - input.FinalCalibrations.Count} calibrations excluded";
        return Task.FromResult(input);
    }
}
