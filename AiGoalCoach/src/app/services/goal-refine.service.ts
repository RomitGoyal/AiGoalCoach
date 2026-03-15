import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { HttpResponse } from '@angular/common/http';
import { GoalRefinementRequest, GoalRefinementResponse } from '../models/goal-refine.models';

@Injectable({
  providedIn: 'root'
})
export class GoalRefineService {
  private readonly baseUrl = 'http://localhost:5010/api';

  constructor(private http: HttpClient) { }

  refineGoal(goal: string): Observable<HttpResponse<GoalRefinementResponse>> {
    const request: GoalRefinementRequest = { goal };
    return this.http.post<GoalRefinementResponse>(`${this.baseUrl}/goal/refine`, request, { observe: 'response' });
  }

  saveGoal(goal: GoalRefinementResponse): Observable<GoalRefinementResponse> {
    return this.http.post<GoalRefinementResponse>(`${this.baseUrl}/goals`, goal);
  }

  getGoals(): Observable<GoalRefinementResponse[]> {
    return this.http.get<GoalRefinementResponse[]>(`${this.baseUrl}/goals`);
  }
}
