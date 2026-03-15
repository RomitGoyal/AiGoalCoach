using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AIGoalCoach.API.Models;

public class TelemetryEvent
{
    [Key]
    public int Id { get; set; }
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    [JsonIgnore]
    public string EventType { get; set; } = "AI_GOAL_REFINEMENT";
    
    public string Status { get; set; } = "SUCCESS";
    
    [MaxLength(1000)]
    public string InputGoal { get; set; } = string.Empty;
    
    public int? PromptTokens { get; set; }
    
    public int? CompletionTokens { get; set; }
    
    public int? TotalTokens { get; set; }
    
    public double LatencyMs { get; set; }
    
    [MaxLength(1000)]
    public string? OutputRefinedGoal { get; set; }
    
    public int? ConfidenceScore { get; set; }
    
    [MaxLength(500)]
    public string? Error { get; set; }
    
    public double EstimatedCostUsd { get; set; }
}
