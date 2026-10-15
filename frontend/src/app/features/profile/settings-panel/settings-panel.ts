
import { Component, inject, signal } from '@angular/core';
import { UiService } from '../../../core/services/ui.service';
import { ThemeService } from '../../../core/services/theme.service';

@Component({
  selector: 'app-settings-panel',
  imports: [],
  templateUrl: './settings-panel.html',
  styleUrl: './settings-panel.css',
})
export class SettingsPanel {
  private readonly themeService = inject(ThemeService);
  private readonly uiService = inject(UiService);

  readonly isDarkMode = this.themeService.isDarkMode;

  readonly notificationsEnabled = signal(true);

  onBack(): void {
    this.uiService.showChats();
  }

  onToggleTheme(): void {
    this.themeService.toggleTheme();
  }

  onToggleNotifications(): void {
    this.notificationsEnabled.update((v) => !v);
  }
}


