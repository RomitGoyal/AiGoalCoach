using AIGoalCoach.API.Models;
using AIGoalCoach.API.Data;
using AIGoalCoach.API.Services;
using Microsoft.EntityFrameworkCore;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddCors(builder => builder.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection")!, 
                new MySqlServerVersion(new Version(8, 0, 36))));

        builder.Services.AddScoped<ITelemetryService, TelemetryService>();

        builder.WebHost.UseUrls("http://localhost:5010");

        var config = builder.Configuration;
        var aiProvider = config["Ai:Provider"] ?? "gemini";
        var apiKey = config["Ai:ApiKey"] ?? config["Gemini:ApiKey"] ?? throw new InvalidOperationException("AI ApiKey missing");
        builder.Services.AddSingleton<IAiGoalRefiner>(sp =>
        {
            return aiProvider.ToLowerInvariant() switch
            {
                "openai" => new OpenAiGoalRefiner(apiKey, config["Ai:Model"] ?? "gpt-4o-mini"),
                "gemini" or _ => new GeminiGoalRefiner(apiKey)
            };
        });

        var app = builder.Build();

        app.UseCors();

        // Goals API endpoints
        app.MapPost("/api/goals", async (Goal goal, AppDbContext db) =>
        {
            db.Goals.Add(goal);
            await db.SaveChangesAsync();
            return Results.Created($"/api/goals/{goal.Id}", goal);
        });

        app.MapGet("/api/goals", async (AppDbContext db) =>
        {
            var goals = await db.Goals.OrderByDescending(g => g.CreatedDate).Take(50).ToListAsync();
            return Results.Ok(goals);
        });

        app.MapPost("/api/goal/refine", async (GoalRefinementRequest request, IAiGoalRefiner refiner, ITelemetryService telemetry) =>
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            var goal = request.Goal?.Trim() ?? "";

            Console.WriteLine($"\n=== TELEMETRY INPUT ===");
            Console.WriteLine($"Goal: '{goal}'");

            if (!AIGoalCoach.API.GoalRefinementHandler.IsValidInput(goal))
            {
                var telEventBlocked = new AIGoalCoach.API.Models.TelemetryEvent
                {
                    InputGoal = goal,
                    LatencyMs = stopwatch.ElapsedMilliseconds,
                    Status = "BLOCKED_INVALID_INPUT",
                    Error = "Invalid input (empty or too short)"
                };
                await telemetry.LogAiCallAsync(telEventBlocked);
                Console.WriteLine($"Latency: {stopwatch.ElapsedMilliseconds}ms | BLOCKED: Invalid input");
                return Results.BadRequest(new { error = "Enter meaningful goal", confidence = 1 });
            }

            GoalRefinementResponse result;
            try
            {
                result = await refiner.RefineAsync(goal);

                Console.WriteLine($"Latency: {stopwatch.ElapsedMilliseconds}ms");

                // Enhanced guardrails
                if (result.ConfidenceScore <= 3)
                {
                    Console.WriteLine("BLOCKED: Inappropriate content");
                }
                if (result.ConfidenceScore < 5)
                {
                    Console.WriteLine("WARNING: Low confidence or sensitive topic");
                }

                var telEventSuccess = new AIGoalCoach.API.Models.TelemetryEvent
                {
                    InputGoal = goal,
                    LatencyMs = stopwatch.ElapsedMilliseconds,
                    Status = "SUCCESS",
                    PromptTokens = result.PromptTokens,
                    CompletionTokens = result.CompletionTokens,
                    TotalTokens = result.TotalTokens,
                    OutputRefinedGoal = result.RefinedGoal,
                    Error = null,
                    ConfidenceScore = result.ConfidenceScore
                };
                await telemetry.LogAiCallAsync(telEventSuccess);

                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                var telEventError = new AIGoalCoach.API.Models.TelemetryEvent
                {
                    InputGoal = goal,
                    LatencyMs = stopwatch.ElapsedMilliseconds,
                    Status = "ERROR",
                    Error = ex.Message
                };
                await telemetry.LogAiCallAsync(telEventError);

                return Results.Ok(new GoalRefinementResponse
                {
                    RefinedGoal = $"Sorry for inconvenience our system is down, please try again later. Your goal is : {goal}",
                    KeyResults = new[] { "Practise", "Track progress", "Weekly review", "Get feedback" },
                    ConfidenceScore = 0
                });
            }
        });

        app.Run();
    }
}
