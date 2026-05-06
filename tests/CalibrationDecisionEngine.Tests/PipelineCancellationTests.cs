using CalibrationDecisionEngine.Domain.Models;
using CalibrationDecisionEngine.Domain.Steps;
using CalibrationDecisionEngine.Pipeline;
using FluentAssertions;
using Xunit;

namespace CalibrationDecisionEngine.Tests;

public sealed class PipelineCancellationTests
{
    private sealed class CancellationTriggerStep : IPipelineStep<VehicleContext, VehicleContext>
    {
        private readonly CancellationTokenSource _cts;
        public CancellationTriggerStep(CancellationTokenSource cts) => _cts = cts;

        public Task<VehicleContext> ExecuteAsync(VehicleContext input, CancellationToken cancellationToken)
        {
            _cts.Cancel();
            return Task.FromResult(input);
        }
    }

    private sealed class SpyStep : IPipelineStep<VehicleContext, VehicleContext>
    {
        public bool WasExecuted { get; private set; }

        public Task<VehicleContext> ExecuteAsync(VehicleContext input, CancellationToken cancellationToken)
        {
            WasExecuted = true;
            return Task.FromResult(input);
        }
    }

    [Fact]
    public async Task Pipeline_StopsBeforeNextStep_WhenTokenCancelledMidway()
    {
        using var cts = new CancellationTokenSource();
        var spy = new SpyStep();

        var pipeline = new PipelineBuilder<VehicleContext, VehicleContext>()
            .AddStep(new NormalizeInputStep())
            .AddStep(new CancellationTriggerStep(cts))
            .AddStep(spy)
            .Build();

        var ctx = new VehicleContext
        {
            Vin = "TEST",
            Lines = [new EstimateLine { Description = "Replace front camera", Operation = "RPL" }]
        };

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            pipeline.ExecuteAsync(ctx, cts.Token));

        spy.WasExecuted.Should().BeFalse("pipeline must not continue past a cancelled token");
    }
}
