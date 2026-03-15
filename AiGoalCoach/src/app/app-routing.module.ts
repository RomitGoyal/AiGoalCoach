import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { GoalRefineComponent } from './goal-refine/goal-refine.component';

import { ShowGoalsComponent } from './show-goals/show-goals.component';

const routes: Routes = [
  { path: '', redirectTo: '/refine', pathMatch: 'full' },
  { path: 'refine', component: GoalRefineComponent },
  { path: 'goals', component: ShowGoalsComponent }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }

