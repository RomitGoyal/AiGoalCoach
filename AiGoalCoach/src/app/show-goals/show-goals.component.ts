import { Component, OnInit } from '@angular/core';
import { GoalRefineService } from '../services/goal-refine.service';
import { GoalRefinementResponse } from '../models/goal-refine.models';
import { Router } from '@angular/router';

@Component({
  selector: 'app-show-goals',
  templateUrl: './show-goals.component.html',
  styleUrls: ['./show-goals.component.css']
})
export class ShowGoalsComponent implements OnInit {
  goals: GoalRefinementResponse[] = [];
  loading = false;

  constructor(private goalRefineService: GoalRefineService, private router: Router) {}

  ngOnInit() {
    this.loadGoals();
  }

  loadGoals() {
    this.loading = true;
    this.goalRefineService.getGoals().subscribe({
      next: (goals) => {
        this.goals = goals;
        this.loading = false;
      },
      error: (err) => {
        console.error('Failed to load goals:', err);
        this.goals = [];
        this.loading = false;
      }
    });
  }
}
