using CalibrationDecisionEngine.Domain.Dtos;
using CalibrationDecisionEngine.Domain.Models;
using CalibrationDecisionEngine.Pipeline;

namespace CalibrationDecisionEngine.Domain.Steps;

public sealed class BuildReportStep : IPipelineStep<VehicleContext, VehicleContext>
{
    public Task<VehicleContext> ExecuteAsync(VehicleContext input, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        input.Report = new EvaluateResponse
        {
            Vin = input.Vin,
            RequiredCalibrations = input.FinalCalibrations
                .Select(c => new CalibrationResultDto
                {
                    RuleName = c.RuleName,
                    Category = c.Category,
                    Trigger = c.Trigger
                })
                .ToList()
        };

        input.StepNotes = "report built";
        return Task.FromResult(input);
    }
}
