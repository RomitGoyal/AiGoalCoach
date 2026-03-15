using AIGoalCoach.API.Models;

namespace AIGoalCoach.API.Services;

public interface IAiGoalRefiner
{
    Task<GoalRefinementResponse> RefineAsync(string goal);
}
