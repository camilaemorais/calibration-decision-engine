using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CalibrationDecisionEngine.Tests;

public sealed class EndToEndTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public EndToEndTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Evaluate_HappyPath_Returns3CalibrationsAnd4TraceSteps()
    {
        var request = new
        {
            vin = "1HGCM82633A123456",
            headers = new[] { "Front Bumper", "Windshield", "Front Radar" },
            lines = new[]
            {
                new { description = "Replace front camera", operation = "RPL" },
                new { description = "Calibrate front radar",  operation = "CAL" },
                new { description = "R&I windshield",         operation = "RI"  },
                new { description = "Replace front bumper",   operation = "RPL" }
            }
        };

        var response = await _client.PostAsJsonAsync("/vehicle/evaluate", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        body.GetProperty("vin").GetString().Should().Be("1HGCM82633A123456");

        var calibrations = body.GetProperty("requiredCalibrations").EnumerateArray().ToList();
        calibrations.Should().HaveCount(3);
        calibrations.Select(c => c.GetProperty("ruleName").GetString())
            .Should().BeEquivalentTo(new[]
            {
                "Front Camera Calibration",
                "Front Radar Calibration",
                "Windshield Calibration"
            });

        var trace = body.GetProperty("trace").EnumerateArray().ToList();
        trace.Should().HaveCount(4);
        trace.Select(t => t.GetProperty("step").GetString())
            .Should().ContainInOrder(
                "NormalizeInputStep",
                "MatchRulesStep",
                "ApplyExclusionsStep",
                "BuildReportStep");
    }

    [Fact]
    public async Task Evaluate_MissingVin_Returns400ProblemDetails()
    {
        var request = new
        {
            vin = "",
            lines = new[] { new { description = "Replace front camera", operation = "RPL" } }
        };

        var response = await _client.PostAsJsonAsync("/vehicle/evaluate", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("status").GetInt32().Should().Be(400);
    }

    [Fact]
    public async Task Evaluate_EmptyLines_Returns400ProblemDetails()
    {
        var request = new
        {
            vin = "1HGCM82633A123456",
            lines = Array.Empty<object>()
        };

        var response = await _client.PostAsJsonAsync("/vehicle/evaluate", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
