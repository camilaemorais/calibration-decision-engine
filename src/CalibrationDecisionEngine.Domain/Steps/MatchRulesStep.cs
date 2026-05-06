using System.Text.RegularExpressions;
using CalibrationDecisionEngine.Domain.Models;
using CalibrationDecisionEngine.Pipeline;
using Microsoft.Extensions.Configuration;

namespace CalibrationDecisionEngine.Domain.Steps;

public sealed class MatchRulesStep : IPipelineStep<VehicleContext, VehicleContext>
{
    private readonly IReadOnlyList<CalibrationRule> _rules;
    private readonly IConfiguration _configuration;

    public MatchRulesStep(IReadOnlyList<CalibrationRule> rules, IConfiguration configuration)
    {
        _rules = rules;
        _configuration = configuration;
    }

    public async Task<VehicleContext> ExecuteAsync(VehicleContext input, CancellationToken cancellationToken)
    {
        var exactWord = string.Equals(
            _configuration["Matching:Mode"], "ExactWord", StringComparison.OrdinalIgnoreCase);

        var tasks = _rules.Select(rule => Task.Run(() => MatchRule(rule, input, exactWord), cancellationToken));
        await Task.WhenAll(tasks).ConfigureAwait(false);

        input.StepNotes = $"{input.MatchedCalibrations.Count} matches across {_rules.Count} rules";
        return input;
    }

    private static void MatchRule(CalibrationRule rule, VehicleContext ctx, bool exactWord)
    {
        foreach (var keyword in rule.MatchKeywords)
        {
            var matchingLine = ctx.Lines.FirstOrDefault(l => Matches(l.Description, keyword, exactWord));
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

            var matchingHeader = ctx.Headers.FirstOrDefault(h => Matches(h, keyword, exactWord));
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

    private static bool Matches(string text, string keyword, bool exactWord)
    {
        if (!exactWord)
            return text.Contains(keyword, StringComparison.OrdinalIgnoreCase);

        return Regex.IsMatch(text, $@"\b{Regex.Escape(keyword)}\b", RegexOptions.IgnoreCase);
    }
}
