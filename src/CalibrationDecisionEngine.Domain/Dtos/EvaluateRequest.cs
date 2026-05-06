using System.ComponentModel.DataAnnotations;

namespace CalibrationDecisionEngine.Domain.Dtos;

public sealed class EvaluateRequest
{
    [Required]
    public string Vin { get; init; } = string.Empty;

    public IReadOnlyList<string> Headers { get; init; } = [];

    [Required, MinLength(1)]
    public IReadOnlyList<EstimateLineDto> Lines { get; init; } = [];
}

public sealed class EstimateLineDto
{
    public string Description { get; init; } = string.Empty;
    public string Operation { get; init; } = string.Empty;
}
