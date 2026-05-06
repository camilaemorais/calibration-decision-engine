using CalibrationDecisionEngine.Domain.Models;
using CalibrationDecisionEngine.Pipeline;

namespace CalibrationDecisionEngine.Domain.Steps;

public sealed class MatchRulesStep : IPipelineStep<VehicleContext, VehicleContext>
{
    private readonly IReadOnlyList<CalibrationRule> _rules;

    public MatchRulesStep(IReadOnlyList<CalibrationRule> rules)
    {
        _rules = rules;
    }

    public async Task<VehicleContext> ExecuteAsync(VehicleContext input, CancellationToken cancellationToken)
    {
        var tasks = _rules.Select(rule => Task.Run(() => MatchRule(rule, input), cancellationToken));
        await Task.WhenAll(tasks).ConfigureAwait(false);

        input.StepNotes = $"{input.MatchedCalibrations.Count} matches across {_rules.Count} rules";
        return input;
    }

    private static void MatchRule(CalibrationRule rule, VehicleContext ctx)
    {
        foreach (var keyword in rule.MatchKeywords)
        {
            var matchingLine = ctx.Lines.FirstOrDefault(l =>
                l.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase));

            if (matchingLine is not null)
            {
                ctx.MatchedCalibrations.Add(new MatchedCalibration
                {
                    RuleName = rule.Name,
                    Category = rule.Category,
                    Trigger = matchingLine.Description
                });
                return;
            }

            var matchingHeader = ctx.Headers.FirstOrDefault(h =>
                h.Contains(keyword, StringComparison.OrdinalIgnoreCase));

            if (matchingHeader is not null)
            {
                ctx.MatchedCalibrations.Add(new MatchedCalibration
                {
                    RuleName = rule.Name,
                    Category = rule.Category,
                    Trigger = matchingHeader
                });
                return;
            }
        }
    }
}
