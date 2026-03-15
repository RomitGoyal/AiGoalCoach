import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { HttpResponse } from '@angular/common/http';
import { GoalRefinementRequest, GoalRefinementResponse } from '../models/goal-refine.models';

@Injectable({
  providedIn: 'root'
})
export class GoalRefineService {
  private apiUrl = 'http://localhost:5010/api/goal/refine';  // Backend endpoint (avoid AirPlay port 5000)

  constructor(private http: HttpClient) { }

  refineGoal(goal: string): Observable<HttpResponse<any>> {
    const request: GoalRefinementRequest = { goal };
    return this.http.post<any>(this.apiUrl, request, { observe: 'response' });
  }
}

