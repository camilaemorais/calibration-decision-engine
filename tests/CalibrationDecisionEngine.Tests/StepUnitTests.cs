using CalibrationDecisionEngine.Domain.Models;
using CalibrationDecisionEngine.Domain.Steps;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace CalibrationDecisionEngine.Tests;

public sealed class NormalizeInputStepTests
{
    private static VehicleContext MakeContext(params (string desc, string op)[] lines) => new()
    {
        Vin = "TEST",
        Lines = lines.Select(l => new EstimateLine { Description = l.desc, Operation = l.op }).ToList()
    };

    [Fact]
    public async Task Execute_TrimsAndUppercasesOperation()
    {
        var ctx = MakeContext(("  Replace front camera  ", "  rpl  "));
        var step = new NormalizeInputStep();

        var result = await step.ExecuteAsync(ctx, default);

        result.Lines[0].Description.Should().Be("Replace front camera");
        result.Lines[0].Operation.Should().Be("RPL");
    }

}

public sealed class MatchRulesStepTests
{
    private static readonly IConfiguration DefaultConfig = new ConfigurationBuilder().Build();

    private static CalibrationRule MakeRule(string name, params string[] keywords) => new()
    {
        Name = name,
        Category = "Test",
        MatchKeywords = keywords,
        Excludes = []
    };

    [Fact]
    public async Task Execute_MatchesByLineDescription()
    {
        var rules = new List<CalibrationRule> { MakeRule("Camera Rule", "front camera") };
        var ctx = new VehicleContext
        {
            Vin = "TEST",
            Lines = [new EstimateLine { Description = "Replace front camera", Operation = "RPL" }]
        };

        var result = await new MatchRulesStep(rules, DefaultConfig).ExecuteAsync(ctx, default);

        result.MatchedCalibrations.Should().ContainSingle()
            .Which.RuleName.Should().Be("Camera Rule");
    }

    [Fact]
    public async Task Execute_MatchesByHeader_WhenNoLineMatches()
    {
        var rules = new List<CalibrationRule> { MakeRule("Bumper Rule", "front bumper") };
        var ctx = new VehicleContext
        {
            Vin = "TEST",
            Headers = ["Front Bumper"],
            Lines = [new EstimateLine { Description = "Unrelated work", Operation = "RPL" }]
        };

        var result = await new MatchRulesStep(rules, DefaultConfig).ExecuteAsync(ctx, default);

        result.MatchedCalibrations.Should().ContainSingle()
            .Which.Trigger.Should().Be("Front Bumper");
    }

    [Fact]
    public async Task Execute_50ParallelRules_AllMatchesRecordedWithoutRaceCondition()
    {
        const int ruleCount = 50;
        var rules = Enumerable.Range(1, ruleCount)
            .Select(i => MakeRule($"Rule{i}", $"keyword{i}"))
            .ToList();

        var lines = Enumerable.Range(1, ruleCount)
            .Select(i => new EstimateLine { Description = $"desc keyword{i}", Operation = "RPL" })
            .ToList();

        var ctx = new VehicleContext { Vin = "TEST", Lines = lines };

        var result = await new MatchRulesStep(rules, DefaultConfig).ExecuteAsync(ctx, default);

        result.MatchedCalibrations.Should().HaveCount(ruleCount);
        result.MatchedCalibrations.Select(c => c.RuleName)
            .Should().BeEquivalentTo(rules.Select(r => r.Name));
    }

    [Fact]
    public async Task Execute_ExactWordMode_DoesNotMatchPartialWord()
    {
        var rules = new List<CalibrationRule> { MakeRule("Radar Rule", "radar") };
        var ctx = new VehicleContext
        {
            Vin = "TEST",
            Lines = [new EstimateLine { Description = "radarmaster unit replaced", Operation = "RPL" }]
        };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Matching:Mode"] = "ExactWord" })
            .Build();

        var result = await new MatchRulesStep(rules, config).ExecuteAsync(ctx, default);

        result.MatchedCalibrations.Should().BeEmpty();
    }
}

public sealed class ApplyExclusionsStepTests
{
    [Fact]
    public async Task Execute_RemovesCalibrationListedInExcludes()
    {
        var rules = new List<CalibrationRule>
        {
            new() { Name = "Bumper Rule", Category = "Bumper", MatchKeywords = ["bumper"], Excludes = ["Windshield Rule"] },
            new() { Name = "Windshield Rule", Category = "Windshield", MatchKeywords = ["windshield"], Excludes = [] }
        };

        var ctx = new VehicleContext { Vin = "TEST", Lines = [new EstimateLine { Description = "test", Operation = "RPL" }] };
        ctx.MatchedCalibrations.Add(new MatchedCalibration { RuleName = "Bumper Rule", Category = "Bumper", Trigger = "bumper" });
        ctx.MatchedCalibrations.Add(new MatchedCalibration { RuleName = "Windshield Rule", Category = "Windshield", Trigger = "windshield" });

        var result = await new ApplyExclusionsStep(rules).ExecuteAsync(ctx, default);

        result.FinalCalibrations.Should().ContainSingle()
            .Which.RuleName.Should().Be("Bumper Rule");
    }

    [Fact]
    public async Task Execute_RemovesBumperCalibration_WhenNoAirbagLinePresent()
    {
        var rules = new List<CalibrationRule>
        {
            new() { Name = "Bumper Sensor Calibration", Category = "Bumper", MatchKeywords = ["bumper"], Excludes = [] }
        };

        var ctx = new VehicleContext
        {
            Vin = "TEST",
            Lines = [new EstimateLine { Description = "Replace front bumper", Operation = "RPL" }]
        };
        ctx.MatchedCalibrations.Add(new MatchedCalibration { RuleName = "Bumper Sensor Calibration", Category = "Bumper", Trigger = "Replace front bumper" });

        var result = await new ApplyExclusionsStep(rules).ExecuteAsync(ctx, default);

        result.FinalCalibrations.Should().BeEmpty();
    }

    [Fact]
    public async Task Execute_KeepsBumperCalibration_WhenAirbagLinePresent()
    {
        var rules = new List<CalibrationRule>
        {
            new() { Name = "Bumper Sensor Calibration", Category = "Bumper", MatchKeywords = ["bumper"], Excludes = [] }
        };

        var ctx = new VehicleContext
        {
            Vin = "TEST",
            Lines =
            [
                new EstimateLine { Description = "Replace front bumper", Operation = "RPL" },
                new EstimateLine { Description = "Replace airbag module", Operation = "RPL" }
            ]
        };
        ctx.MatchedCalibrations.Add(new MatchedCalibration { RuleName = "Bumper Sensor Calibration", Category = "Bumper", Trigger = "Replace front bumper" });

        var result = await new ApplyExclusionsStep(rules).ExecuteAsync(ctx, default);

        result.FinalCalibrations.Should().ContainSingle();
    }
}

public sealed class BuildReportStepTests
{
    [Fact]
    public async Task Execute_BuildsReportWithVinAndCalibrations()
    {
        var ctx = new VehicleContext { Vin = "1HGCM82633A123456" };
        ctx.FinalCalibrations.Add(new MatchedCalibration
        {
            RuleName = "Front Camera Calibration",
            Category = "Camera",
            Trigger = "Replace front camera"
        });

        var result = await new BuildReportStep().ExecuteAsync(ctx, default);

        result.Report.Should().NotBeNull();
        result.Report!.Vin.Should().Be("1HGCM82633A123456");
        result.Report.RequiredCalibrations.Should().ContainSingle()
            .Which.RuleName.Should().Be("Front Camera Calibration");
        result.StepNotes.Should().Be("report built");
    }
}
