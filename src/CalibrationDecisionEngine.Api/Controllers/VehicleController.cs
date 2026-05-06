using CalibrationDecisionEngine.Domain.Dtos;
using CalibrationDecisionEngine.Domain.Models;
using CalibrationDecisionEngine.Pipeline;
using Microsoft.AspNetCore.Mvc;

namespace CalibrationDecisionEngine.Api.Controllers;

[ApiController]
[Route("vehicle")]
public sealed class VehicleController : ControllerBase
{
    private readonly IPipeline<VehicleContext, VehicleContext> _pipeline;

    public VehicleController(IPipeline<VehicleContext, VehicleContext> pipeline)
    {
        _pipeline = pipeline;
    }

    [HttpPost("evaluate")]
    public async Task<IActionResult> Evaluate(
        [FromBody] EvaluateRequest request,
        CancellationToken cancellationToken)
    {
        var context = new VehicleContext
        {
            Vin = request.Vin,
            Headers = request.Headers,
            Lines = request.Lines
                .Select(l => new EstimateLine { Description = l.Description, Operation = l.Operation })
                .ToList()
        };

        PipelineResult<VehicleContext> result;
        try
        {
            result = await _pipeline.ExecuteAsync(context, cancellationToken);
        }
        catch (PipelineExecutionException ex)
        {
            return Problem(
                title: "Pipeline execution failed",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError);
        }

        var report = result.Output.Report!;
        return Ok(new EvaluateResponse
        {
            Vin = report.Vin,
            RequiredCalibrations = report.RequiredCalibrations,
            Trace = result.Trace
                .Select(t => new TraceEntryDto { Step = t.Step, DurationMs = t.DurationMs, Notes = t.Notes })
                .ToList()
        });
    }
}
