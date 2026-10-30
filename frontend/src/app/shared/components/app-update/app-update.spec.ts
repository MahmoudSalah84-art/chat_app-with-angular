import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AppUpdate } from './app-update';

describe('AppUpdate', () => {
  let component: AppUpdate;
  let fixture: ComponentFixture<AppUpdate>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppUpdate],
    }).compileComponents();

    fixture = TestBed.createComponent(AppUpdate);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
