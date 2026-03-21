using AIGoalCoach.API.Models;
using AIGoalCoach.API.Data;
using AIGoalCoach.API.Services;
using Microsoft.EntityFrameworkCore;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.WebHost.UseUrls("http://localhost:5010");

        builder.Services.AddEndpointsApiExplorer();
        
        //CORS configuration - allow all for development, consider restricting in production
        builder.Services.AddCors(builder => builder.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
        
        //DB Context configuration - using MySQL with connection string from appsettings.json
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection")!, 
                new MySqlServerVersion(new Version(8, 0, 36))));

        var config = builder.Configuration;
        var aiProvider = config["Ai:Provider"] ?? "gemini";
        var apiKey = config["Ai:ApiKey"] ?? config["Gemini:ApiKey"] ?? throw new InvalidOperationException("AI ApiKey missing");
        
        //Singleton service for AI Goal Refiner, implementation chosen based on configuration (OpenAI or Gemini)
        builder.Services.AddSingleton<IAiGoalRefiner>(sp =>
        {
            return aiProvider.ToLowerInvariant() switch
            {
                //factory pattern to choose AI provider implementation based on config, defaults to Gemini if not specified or unrecognized
                "openai" => new OpenAiGoalRefiner(apiKey, config["Ai:Model"] ?? "gpt-4o-mini"),
                "gemini" or _ => new GeminiGoalRefiner(apiKey)
            };
        });

        //scoped service for telemetry, can be injected into endpoints to log AI call details
        builder.Services.AddScoped<ITelemetryService, TelemetryService>();

        var app = builder.Build();

        app.UseCors();

        // endpoint to create a new goal, accepts Goal object in request body, saves to database and returns created goal with 201 status
        app.MapPost("/api/goals", async (Goal goal, AppDbContext db) =>
        {
            db.Goals.Add(goal);
            await db.SaveChangesAsync();
            return Results.Created($"/api/goals/{goal.Id}", goal);
        });

        //endpoint to retrieve recent goals, returns list of goals ordered by creation date, limited to 50 most recent
        app.MapGet("/api/goals", async (AppDbContext db) =>
        {
            var goals = await db.Goals.OrderByDescending(g => g.CreatedDate).Take(50).ToListAsync();
            return Results.Ok(goals);
        });

        //endpoint to refine a goal using AI, accepts GoalRefinementRequest with a goal string, validates input, calls AI refiner service, logs telemetry, and returns refined goal or error message
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
