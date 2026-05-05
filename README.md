# calibration-decision-engine

Rule-based pipeline to determine which ADAS calibrations are required based on a repair estimate.
Stack: .NET 8 + ASP.NET Core, xUnit for testing.

## Run

```bash
dotnet restore
dotnet build
dotnet run --project src/CalibrationDecisionEngine.Api
```

## Tests

```bash
dotnet test
```

---

WIP: full README (design decisions, trade-offs, what was left out) will be included with the final commit.