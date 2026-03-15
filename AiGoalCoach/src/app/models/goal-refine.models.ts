export interface GoalRefinementRequest {
  goal: string;
}

export interface GoalRefinementResponse {
  refined_goal: string;
  key_results: string[];
  confidence_score: number;
}

