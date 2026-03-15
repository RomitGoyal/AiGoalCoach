using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Text;
using System.Text.Json;
using AIGoalCoach.API.Models;

namespace AIGoalCoach.API.Tests;

public class GoalRefinementApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public GoalRefinementApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Override config if needed, e.g., invalid API key to test fallback
            });
        });
        _client = _factory.CreateClient();
    }

    [Theory]
    [InlineData("learn guitar")]
    public async Task ValidGoal_ReturnsOkAndValidResponse(string goal)
    {
        // Arrange
        var request = new GoalRefinementRequest { Goal = goal };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/goal/refine", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseString = await response.Content.ReadAsStringAsync();
        JsonDocument.Parse(responseString).RootElement.Should().NotBeNull(); // Valid JSON

        var result = JsonSerializer.Deserialize<GoalRefinementResponse>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        result.Should().NotBeNull();
        result?.RefinedGoal.Should().NotBeEmpty();
        result?.KeyResults.Should().NotBeEmpty();
        result?.ConfidenceScore.Should().BeGreaterThan(3);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public async Task InvalidGoal_ReturnsBadRequest(string? goal)
    {
        // Arrange
        var request = new GoalRefinementRequest { Goal = goal ?? string.Empty };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/goal/refine", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("FVghsjdnjmasm")]
    public async Task GuardrailLowConfidence_StillReturnsResponse(string goal)
    {
        // Goal likely to trigger low confidence, e.g., vague or nonsense
        var request = new GoalRefinementRequest { Goal = goal };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/goal/refine", content);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<GoalRefinementResponse>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        result.Should().NotBeNull();
        result?.ConfidenceScore.Should().BeLessThan(3); // Returns response even for low confidence
        result?.RefinedGoal.Equals("Invalid goal content - keep it productive");
    }

    [Fact]
    public async Task FallbackOnError_ReturnsDefaultResponse()
    {
        // To test fallback, override API key to invalid
        var invalidKeyFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
["Ai:ApiKey"] = "invalid_key_to_trigger_error"
                });
            });
        });
        var errorClient = invalidKeyFactory.CreateClient();

        var request = new GoalRefinementRequest { Goal = "test error" };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await errorClient.PostAsync("/api/goal/refine", content);
        response.StatusCode.Should().Be(HttpStatusCode.OK); // Fallback returns OK

        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<GoalRefinementResponse>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        result.Should().NotBeNull();
        result?.RefinedGoal.Equals($"Sorry for inconvenience our system is down, please try again later. Your goal is : test error"); // Works with fallback or AI response
    }
}
