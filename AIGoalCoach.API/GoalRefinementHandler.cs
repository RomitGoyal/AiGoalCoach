using AIGoalCoach.API.Models;
using System.Text.Json;

namespace AIGoalCoach.API;

public class GoalRefinementHandler
{
    public static bool IsValidInput(string goal)
    {
        return !string.IsNullOrWhiteSpace(goal) && goal.Length >= 3;
    }

    public static GoalRefinementResponse ParseGeminiResponse(string jsonResponse)
    {
        return JsonSerializer.Deserialize<GoalRefinementResponse>(jsonResponse ?? "{}", new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new GoalRefinementResponse();
    }

    public static int ApplyGuardrails(GoalRefinementResponse response)
    {
        if (response.ConfidenceScore <= 3)
        {
            // Blocked
            return 1;
        }
        return response.ConfidenceScore;
    }
}
