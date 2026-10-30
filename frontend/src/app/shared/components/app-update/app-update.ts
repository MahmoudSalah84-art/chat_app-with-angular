import { Component, inject } from '@angular/core';
import {AppUpdateService} from '../../../core/index'

@Component({
  selector: 'app-update',
  imports: [],
  templateUrl: './app-update.html',
  styleUrl: './app-update.css',
})
export class AppUpdate {

  updateService = inject(AppUpdateService);
}