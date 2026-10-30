import { Injectable, inject, signal } from '@angular/core';
import { SwUpdate } from '@angular/service-worker';

@Injectable({
  providedIn: 'root'
})
export class AppUpdateService {

  private swUpdate = inject(SwUpdate);

  readonly updateAvailable = signal(false);

  constructor() {
    if (!this.swUpdate.isEnabled) return; // there isn't sw

    this.swUpdate.versionUpdates.subscribe(event => {
      if (event.type === 'VERSION_READY')
        this.updateAvailable.set(true);
    });
  }

  async updateApp(): Promise<void> {
    try {
      const activated = await this.swUpdate.activateUpdate();

      if (activated)  window.location.reload();

    } catch (error) {
      console.error('Update failed:', error);
    }
  }
}