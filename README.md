# AI Goal Coach

AI Goal Coach is a web application that helps users refine their goals using Artificial Intelligence. Users can input a goal, and the application will provide a refined version of the goal along with key results to achieve it.

## Features

*   **Goal Refinement:** Utilizes AI (powered by Gemini or OpenAI) to refine user goals, making them more specific, measurable, achievable, relevant, and time-bound (SMART).
*   **Key Results:** Generates actionable key results for each refined goal.
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

**SQL Injection Protection:** EF Core parameterized queries, input validation.

## Why SQL Telemetry Logging?
* **Queryable analytics**: `SELECT AVG(LatencyMs), AVG(EstimatedCostUsd) FROM TelemetryEvents`
* **Retention**: Easy cleanup `DELETE WHERE Timestamp < DATE_SUB(NOW(), INTERVAL 90 DAY)`
* **Backup/scale**: Cloud SQL snapshots, read replicas
* **Fallback**: JSONL + DB dual-write

**Metrics tracked:** InputGoal, tokens, latency, confidence, cost USD.

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
