import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NewGroup } from './new-group';

describe('NewGroup', () => {
  let component: NewGroup;
  let fixture: ComponentFixture<NewGroup>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NewGroup],
    }).compileComponents();

    fixture = TestBed.createComponent(NewGroup);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
