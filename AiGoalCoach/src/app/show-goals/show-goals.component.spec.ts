import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ShowGoalsComponent } from './show-goals.component';

describe('ShowGoalsComponent', () => {
  let component: ShowGoalsComponent;
  let fixture: ComponentFixture<ShowGoalsComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [ShowGoalsComponent]
    });
    fixture = TestBed.createComponent(ShowGoalsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
