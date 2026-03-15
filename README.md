# AI Goal Coach

AI Goal Coach is a web application that helps users refine their goals using Artificial Intelligence. Users can input a goal, and the application will provide a refined version of the goal along with key results to achieve it.

## Features

*   **Goal Refinement:** Utilizes AI (powered by Gemini or OpenAI) to refine user goals, making them more specific, measurable, achievable, relevant, and time-bound (SMART).
*   **Key Results:** Generates actionable key results for each refined goal.
*   **Save Goals:** Allows users to save their refined goals for future reference.
*   **View Saved Goals:** A dedicated page to view all the saved goals.

## Technologies Used

### Backend

*   .NET 8 Web API
*   C#
*   Swagger for API documentation
*   Configurable AI provider (Gemini or OpenAI)

### Frontend

*   Angular 16
*   TypeScript
*   Bootstrap for styling (or other, please specify if you know)

## Getting Started

To get the application up and running on your local machine, follow these steps.

### Prerequisites

*   [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
*   [Node.js and npm](https://nodejs.org/en/)
*   [Angular CLI](https://angular.io/cli)

### Backend Setup

1.  Navigate to the `AIGoalCoach.API` directory:
    ```bash
    cd AIGoalCoach.API
    ```
2.  Restore the dependencies:
    ```bash
    dotnet restore
    ```
3.  Configure your AI provider in `appsettings.json`. You can choose between "gemini" and "openai" and you must provide an `ApiKey`.

    ```json
    {
      "Ai": {
        "Provider": "gemini",
        "ApiKey": "YOUR_GEMINI_API_KEY"
      }
    }
    ```
    or
    ```json
    {
      "Ai": {
        "Provider": "openai",
        "ApiKey": "YOUR_OPENAI_API_KEY",
        "Model": "gpt-4o-mini"
      }
    }
    ```
4.  Run the backend server:
    ```bash
    dotnet run
    ```
    The API will be running on `http://localhost:5010`. 

### Frontend Setup

1.  Navigate to the `AiGoalCoach` directory:
    ```bash
    cd AiGoalCoach
    ```
2.  Install the dependencies:
    ```bash
    npm install
    ```
3.  Run the frontend development server:
    ```bash
    ng serve
    ```
    The application will be running on `http://localhost:4200`.

## API Endpoint

The backend exposes the following API endpoint:

### Refine Goal

*   **URL:** `/api/goal/refine`
*   **Method:** `POST`
*   **Body:**
    ```json
    {
      "goal": "I want to be a better programmer"
    }
    ```
*   **Success Response (200 OK):**
    ```json
    {
      "refinedGoal": "Become a more proficient programmer within the next 6 months by contributing to open-source projects and completing an advanced programming course.",
      "keyResults": [
        "Contribute to at least 2 open-source projects.",
        "Complete an advanced programming course on a platform like Coursera or edX.",
        "Build a personal project that showcases your new skills.",
        "Dedicate 10 hours per week to deliberate practice."
      ],
      "confidenceScore": 9
    }
    ```
*   **Error Response (400 Bad Request):**
    ```json
    {
      "error": "Enter meaningful goal",
      "confidence": 1
    }
    ```

## Project Structure

The project is divided into two main parts:

*   `AIGoalCoach.API`: The .NET backend API that handles the goal refinement logic.
*   `AiGoalCoach`: The Angular frontend application that provides the user interface.
*   `AIGoalCoach.API.Tests`: Contains tests for the backend API.

The solution is managed by the `AI Goal Coach.sln` solution file.

## Scalability

The backend is designed to handle a large number of users (10,000+) with high performance and reliability. Here's how:

*   **High Concurrency**: The application can handle thousands of simultaneous requests. It is built on .NET 8, which is designed for high-performance, and can be deployed in a containerized environment like Kubernetes to scale to millions of requests.
*   **Efficient Memory Usage**: Key services like `ITelemetryService` and `IAiGoalRefiner` are registered as singletons, which means only one instance of these services is created and shared across all requests. This minimizes memory overhead. `HttpClient` is used to create a pool of connections that can be reused for subsequent requests.
*   **AI Provider Rate Limiting**: The application is designed to work with AI providers like Gemini, which has a rate limit of 15 requests per minute on the free tier. The application can be configured to handle this rate limit and can be upgraded to a paid tier for higher limits.
*   **Resiliency**: The application has a fallback mechanism that provides an instant response if the AI provider is down, ensuring a good user experience.
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
