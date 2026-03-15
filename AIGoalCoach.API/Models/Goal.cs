using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using AIGoalCoach.API.Models;

namespace AIGoalCoach.API.Models;

public class Goal
{
    public int Id { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    [JsonPropertyName("refined_goal")]
    public required string RefinedGoal { get; set; }
    
    [JsonPropertyName("key_results")]
    public required string[] KeyResults { get; set; } = Array.Empty<string>();
    
    [JsonPropertyName("confidence_score")]
    public int ConfidenceScore { get; set; }
}
