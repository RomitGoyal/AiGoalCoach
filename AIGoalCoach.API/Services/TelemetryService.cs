using System.Text.Json;
using System.Text;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using AIGoalCoach.API.Data;
using AIGoalCoach.API.Models;

namespace AIGoalCoach.API.Services;

public interface ITelemetryService
{
    Task LogAiCallAsync(TelemetryEvent evt);
}

public class TelemetryService : ITelemetryService
{
    private readonly AppDbContext _dbContext;
    private readonly string? _logPath;
    private const double INPUT_TOKEN_COST_USD = 0.000075; // Gemini 1.5 Flash input per token
    private const double OUTPUT_TOKEN_COST_USD = 0.0003; // output per token

    public TelemetryService(AppDbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _logPath = configuration["Telemetry:LogPath"];
        Console.WriteLine($"Telemetry LogPath loaded: '{_logPath ?? "NULL"}'");
    }

    public async Task LogAiCallAsync(TelemetryEvent evt)
    {
        // Compute cost
        var inputCost = (evt.PromptTokens ?? 0) * INPUT_TOKEN_COST_USD;
        var outputCost = (evt.CompletionTokens ?? 0) * OUTPUT_TOKEN_COST_USD;
        var eventWithCost = new TelemetryEvent
        {
            Timestamp = DateTime.UtcNow,
            EventType = evt.EventType,
            Status = evt.Status,
            InputGoal = evt.InputGoal,
            PromptTokens = evt.PromptTokens,
            CompletionTokens = evt.CompletionTokens,
            TotalTokens = evt.TotalTokens,
            LatencyMs = evt.LatencyMs,
            OutputRefinedGoal = evt.OutputRefinedGoal,
            ConfidenceScore = evt.ConfidenceScore,
            Error = evt.Error,
            EstimatedCostUsd = inputCost + outputCost
        };

        var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var json = JsonSerializer.Serialize(eventWithCost, options);

        Console.WriteLine($"\\n=== AI TELEMETRY ===");
        Console.WriteLine(json);

        // Save to SQL
        try
        {
            _dbContext.TelemetryEvents.Add(eventWithCost);
            await _dbContext.SaveChangesAsync();
            Console.WriteLine("Telemetry saved to database");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to save telemetry to DB: {ex.Message}");
        }

        // Fallback to JSONL
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
            }
        }
    }
}
