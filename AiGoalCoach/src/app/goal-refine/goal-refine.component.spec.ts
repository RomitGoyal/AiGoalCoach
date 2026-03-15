import { ComponentFixture, TestBed } from '@angular/core/testing';
import { GoalRefineComponent } from './goal-refine.component';

describe('GoalRefineComponent', () => {
  let component: GoalRefineComponent;
  let fixture: ComponentFixture<GoalRefineComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GoalRefineComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(GoalRefineComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

