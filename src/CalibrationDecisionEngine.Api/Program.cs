using CalibrationDecisionEngine.Domain;
using CalibrationDecisionEngine.Domain.Models;
using CalibrationDecisionEngine.Domain.Steps;
using CalibrationDecisionEngine.Pipeline;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();

var rulesPath = Path.Combine(AppContext.BaseDirectory, "rules.json");
builder.Services.AddDomainServices(rulesPath);

builder.Services.AddSingleton<IPipeline<VehicleContext, VehicleContext>>(sp =>
{
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    return new PipelineBuilder<VehicleContext, VehicleContext>(loggerFactory)
        .AddStep(sp.GetRequiredService<NormalizeInputStep>())
        .AddStep(sp.GetRequiredService<MatchRulesStep>())
        .AddStep(sp.GetRequiredService<ApplyExclusionsStep>())
        .AddStep(sp.GetRequiredService<BuildReportStep>())
        .Build();
});

var app = builder.Build();

app.UseExceptionHandler();
app.MapControllers();

app.Run();

public partial class Program { }
