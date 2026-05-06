using System.Text.Json;
using CalibrationDecisionEngine.Domain.Models;
using CalibrationDecisionEngine.Domain.Steps;
using Microsoft.Extensions.DependencyInjection;

namespace CalibrationDecisionEngine.Domain;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDomainServices(
        this IServiceCollection services,
        string rulesJsonPath)
    {
        var json = File.ReadAllText(rulesJsonPath);
        var rules = JsonSerializer.Deserialize<List<CalibrationRule>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? throw new InvalidOperationException($"rules.json is empty or invalid: {rulesJsonPath}");

        services.AddSingleton<IReadOnlyList<CalibrationRule>>(rules);

        services.AddTransient<NormalizeInputStep>();
        services.AddTransient<MatchRulesStep>();
        services.AddTransient<ApplyExclusionsStep>();
        services.AddTransient<BuildReportStep>();

        return services;
    }
}
