namespace AIGoalCoach.API.Models;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class GoalRefinementResponse
{
    [JsonPropertyName("refined_goal")]
    [Required]
    public string RefinedGoal { get; set; } = string.Empty;
    
    [JsonPropertyName("key_results")]
    [Required]
    public string[] KeyResults { get; set; } = Array.Empty<string>();
    
    [JsonPropertyName("confidence_score")]
    [Required]
    public int ConfidenceScore { get; set; }
    
    [JsonPropertyName("prompt_tokens")]
    public int? PromptTokens { get; set; }

    [JsonPropertyName("completion_tokens")]
    public int? CompletionTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    public int? TotalTokens { get; set; }
}

