import { Component, OnInit } from '@angular/core';
import { GoalRefinementResponse } from '../models/goal-refine.models';
import { Router } from '@angular/router';

@Component({
  selector: 'app-show-goals',
  templateUrl: './show-goals.component.html',
  styleUrls: ['./show-goals.component.css']
})
export class ShowGoalsComponent implements OnInit {
  goals: GoalRefinementResponse[] = [];
  private STORAGE_KEY = 'savedGoals';

  constructor(private router: Router) {}

  ngOnInit() {
    this.loadGoals();
  }

  private loadGoals() {
    const saved = sessionStorage.getItem(this.STORAGE_KEY);
    if (saved) {
      this.goals = JSON.parse(saved);
    }
  }
}

