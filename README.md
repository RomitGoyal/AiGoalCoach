# AI Goal Coach

AI Goal Coach is a web application that helps users refine their goals using Artificial Intelligence. Users can input a goal, and the application will provide a refined version of the goal along with key results to achieve it.

## Features

*   **Goal Refinement:** Utilizes AI (powered by Gemini or OpenAI) to refine user goals, making them more specific, measurable, achievable, relevant, and time-bound (SMART). Users can review and edit the refined goal and key results if needed.
*   **Key Results:** Generates actionable key results for each refined goal.
*   **Human Override:** Users can manually edit the AI-refined goal and key results before saving, allowing customization when the AI misses personal context or additional details.
*   **Save Goals:** Persists refined goals to **Cloud SQL MySQL** database.
*   **View Saved Goals:** Fetches and displays goals from database.
*   **Telemetry Logging:** AI usage metrics logged to both **SQL** and JSONL.

## Technologies Used

### Backend (.NET 8 Minimal API)
* EF Core Code-First (Cloud SQL MySQL `AiGoalCoach` DB)
* **Goals table**: RefinedGoal, KeyResults, ConfidenceScore
* **TelemetryEvents table**: AI call metrics (tokens, latency, cost, confidence)
* Gemini/OpenAI integration
* SQL Injection protected (parameterized EF queries)

### Frontend (Angular 16)
* HttpClient calls to /api/goal/refine, /api/goals (CRUD)

## Database Architecture (Code-First)

**Why Cloud SQL MySQL:**
* Fully managed, scalable (handles 10k+ users)
* High availability, backups, monitoring
* Horizontal scaling for goals/logs

**Optimizations:**
* Indexed CreatedDate (recent first)
* MaxLength validation (performance/security)
* DbSet partitioning ready

**SQL Injection Protection:** 
* EF Core automatically uses parameterized queries, preventing SQL injection attacks by separating SQL code from user input data.
* All inputs validated with `[MaxLength]` attributes and model validation before persisting to database.
* No raw SQL execution or string concatenation used anywhere in the codebase.

## Why SQL Telemetry Logging?
* **Queryable analytics**: `SELECT AVG(LatencyMs), AVG(EstimatedCostUsd) FROM TelemetryEvents`
* **Retention**: Easy cleanup `DELETE WHERE Timestamp < DATE_SUB(NOW(), INTERVAL 90 DAY)`
* **Backup/scale**: Cloud SQL snapshots, read replicas
* **Fallback**: JSONL + DB dual-write

**Metrics tracked:** InputGoal, tokens, latency, confidence, cost USD.

## High Performance & Scalability

The backend is designed to handle a large number of users (10,000+) with high performance and reliability. Here's how:

*   **High Concurrency**: The application can handle thousands of simultaneous requests. It is built on .NET 8, which is designed for high-performance, and can be deployed in a containerized environment like Kubernetes to scale to millions of requests.
*   **Efficient Memory Usage**: Key services like `ITelemetryService` and `IAiGoalRefiner` are registered as scoped, singletons, which means only one instance of these services is created and shared across all requests. This minimizes memory overhead. `HttpClient` is used to create a pool of connections that can be reused for subsequent requests.
*   **AI Provider Rate Limiting**: The application is designed to work with AI providers like Gemini, which has a rate limit of 15 requests per minute on the free tier. The application can be configured to handle this rate limit and can be upgraded to a paid tier for higher limits.
*   **Resiliency**: The application has a fallback mechanism that provides an instant response if the AI provider is down, ensuring a good user experience.
*   **Caching Frequently Accessed Goals**: Implement in-memory or centralized caching (e.g., IMemoryCache in .NET, Redis) for frequently accessed user goals. This reduces database load by 80-90% on repeated reads, provides sub-10ms response times for cached hits, improves reliability during DB maintenance or outages via cache-aside patterns, and enhances accessibility for users on slow/poor networks by minimizing latency and data transfer.
*   **Non-Blocking Telemetry**: The telemetry service writes to a file asynchronously, which means it doesn't block the execution of the request and has a minimal impact on performance.

## Schema Enforcement

The application ensures that the AI's response adheres to a specific JSON schema using a two-pronged approach:

1.  **Prompt Engineering**: The prompt sent to the AI includes a clear example of the desired JSON output format. This guides the model to structure its response accordingly.

    ```json
    {
      "refined_goal": "Specific SMART goal",
      "key_results": ["Action 1", "Action 2", "Action 3", "Action 4"],
      "confidence_score": 8
    }
    ```

2.  **Generation Configuration**: For the Gemini provider, the `generationConfig` is set to `"responseMimeType": "application/json"`. This forces the model to return a valid JSON object, which is more reliable than relying on the prompt alone. For more complex scenarios, the configuration can be extended to include a specific `"response_schema"` to enforce the structure more rigidly.

## AI Model Selection

The application is configured to use `gemini-3.1-flash-lite-preview` by default, and can also be configured to use OpenAI's `gpt-4o-mini`. These models were chosen for several reasons:

*   **Performance**: Both `gemini-3.1-flash-lite-preview` and `gpt-4o-mini` are lightweight and fast models, which is crucial for an interactive application like this. A quick response time is essential for a good user experience.
*   **Cost-Effectiveness**: These models are more affordable than their larger counterparts, which helps to keep the operational costs of the application low.
*   **Balance**: They provide a good balance of performance, cost, and quality for the task of refining goals.

## Getting Started

### Backend
```bash
cd AIGoalCoach.API
dotnet restore
dotnet ef database update  # Apply migrations
dotnet run  # http://localhost:5010
```

### Frontend
```bash
cd AiGoalCoach
npm install
ng serve  # http://localhost:4200
```

## API Endpoints
- `POST /api/goal/refine` → Refine (AI)
- `POST /api/goals` → Save goal (DB)
- `GET /api/goals` → List goals (DB)

## Run & Test
1. Backend (5010), Frontend (4200)
2. Refine goal → Save → View /goals (from DB)
3. AI call → Check `TelemetryEvents` table

**Scalability:** EF + Cloud SQL → production ready!

