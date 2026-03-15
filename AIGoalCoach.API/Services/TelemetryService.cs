using System.Text.Json;
using System.Text;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace AIGoalCoach.API.Services;

public record TelemetryEvent
{
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string EventType { get; init; } = "AI_GOAL_REFINEMENT";
    public string Status { get; init; } = "SUCCESS"; // SUCCESS, ERROR, FALLBACK
    public string InputGoal { get; init; } = string.Empty;
    public int? PromptTokens { get; init; }
    public int? CompletionTokens { get; init; }
    public int? TotalTokens { get; init; }
    public double LatencyMs { get; init; }
    public string? OutputRefinedGoal { get; init; }
    public int? ConfidenceScore { get; init; }
    public string? Error { get; init; }
    public double EstimatedCostUsd { get; init; }
}

public interface ITelemetryService
{
    Task LogAiCallAsync(TelemetryEvent evt);
}

public class TelemetryService : ITelemetryService
{
    private readonly string? _logPath;
    private const double INPUT_TOKEN_COST_USD = 0.000075; // Gemini 1.5 Flash input per token
    private const double OUTPUT_TOKEN_COST_USD = 0.0003; // output per token

    public TelemetryService(IConfiguration configuration)
    {
        _logPath = configuration["Telemetry:LogPath"];
        Console.WriteLine($"Telemetry LogPath loaded: '{_logPath ?? "NULL"}'");
    }

    public async Task LogAiCallAsync(TelemetryEvent evt)
    {
        // Compute cost
        var inputCost = (evt.PromptTokens ?? 0) * INPUT_TOKEN_COST_USD;
        var outputCost = (evt.CompletionTokens ?? 0) * OUTPUT_TOKEN_COST_USD;
        var eventWithCost = evt with { EstimatedCostUsd = inputCost + outputCost };

        var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var json = JsonSerializer.Serialize(eventWithCost, options);

        Console.WriteLine($"\\n=== AI TELEMETRY ===");
        Console.WriteLine(json);

        if (!string.IsNullOrEmpty(_logPath))
        {
            try
            {
                var fullPath = Path.GetFullPath(_logPath);
                Console.WriteLine($"Full log path: {fullPath}");

                var dir = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                await File.AppendAllTextAsync(fullPath, json + "\\n");
                Console.WriteLine($"Successfully appended to log file: {fullPath}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to write to log file {_logPath}: {ex.Message}");
                Console.Error.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}
