import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AppUpdate } from './shared/components/app-update/app-update';

@Component({
  selector: 'app-root',
  imports: [ RouterOutlet, AppUpdate],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {}