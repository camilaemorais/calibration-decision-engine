using System.Collections.Concurrent;
using CalibrationDecisionEngine.Domain.Dtos;
using CalibrationDecisionEngine.Pipeline;

namespace CalibrationDecisionEngine.Domain.Models;

public sealed class VehicleContext : IPipelineContext
{
    public string Vin { get; init; } = string.Empty;
    public IReadOnlyList<string> Headers { get; init; } = [];

    public List<EstimateLine> Lines { get; set; } = [];

    public ConcurrentBag<MatchedCalibration> MatchedCalibrations { get; } = [];

    public List<MatchedCalibration> FinalCalibrations { get; set; } = [];

    public EvaluateResponse? Report { get; set; }

    public string? StepNotes { get; set; }
}
