import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { GoalRefineService } from '../services/goal-refine.service';
import { HttpResponse } from '@angular/common/http';
import { GoalRefinementResponse } from '../models/goal-refine.models';

@Component({
  selector: 'app-goal-refine',
  templateUrl: './goal-refine.component.html',
  styleUrls: ['./goal-refine.component.css']
})
export class GoalRefineComponent {
  goal = '';
  refinement_prompt = '';
  response: GoalRefinementResponse | null = null;
  savedGoals: GoalRefinementResponse[] = [];
  loading = false;
  error: string | null = null;
  private STORAGE_KEY = 'savedGoals';

  constructor(private goalRefineService: GoalRefineService) {
    this.loadSavedGoals();
  }

  refineGoal() {
    if (!this.goal.trim()) return;

    this.loading = true;
    this.error = null;
    this.response = null;

    this.goalRefineService.refineGoal(this.goal.trim()).subscribe({
      next: (resp: HttpResponse<any>) => {
        const body = resp.body;
        if (resp.status === 200 && body) {
          // Handle low confidence
          if ('confidence_score' in body && body.confidence_score < 5) {
            this.response = body as GoalRefinementResponse;
            this.error = `Low confidence (${body.confidence_score}/10). Make it SMART: Specific, who/what/when?`;
          } else {
            this.response = body as GoalRefinementResponse;
            this.error = null;
          }
        } else {
          this.error = 'Unexpected response status: ' + resp.status;
        }
        this.loading = false;
      },
      error: (err) => {
        this.error = err.status === 0 ? 'Connection failed. Check if backend runs on localhost:5000.' : `Request failed: ${err.message}`;
        this.loading = false;
        console.error(err);
      }
    });
  }

  refineFurther() {
    if (!this.refinement_prompt.trim()) return;

    this.goal = `Original Goal: ${this.goal}\nRefinement: ${this.refinement_prompt}`;
    this.refinement_prompt = '';
    this.refineGoal();
  }

  saveGoal() {
    if (!this.response) return;

    this.savedGoals.unshift(this.response);
    sessionStorage.setItem(this.STORAGE_KEY, JSON.stringify(this.savedGoals));
    alert('Goal saved! Total saved: ' + this.savedGoals.length);
    this.response = null;
    this.goal = '';
  }

  private loadSavedGoals() {
    const saved = sessionStorage.getItem(this.STORAGE_KEY);
    if (saved) {
      this.savedGoals = JSON.parse(saved);
    }
  }

  get savedCount() {
    return this.savedGoals.length;
  }
}

