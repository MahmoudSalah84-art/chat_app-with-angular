import { Component, computed, inject } from '@angular/core';
import { UiService } from '../../../core/services/ui.service';
import { ThemeService } from '../../../core/services/theme.service';
import { PushService } from '../../../core/services/push.service';

@Component({
  selector: 'app-settings-panel',
  imports: [],
  templateUrl: './settings-panel.html',
  styleUrl: './settings-panel.css',
})
export class SettingsPanel {
  private readonly themeService = inject(ThemeService);
  private readonly uiService = inject(UiService);
  private readonly push = inject(PushService);

  readonly isDarkMode = this.themeService.isDarkMode;
  readonly notificationsEnabled = this.push.isSubscribed;

  readonly notificationsDisabled = computed(
    () => !this.push.isSupported || this.push.permission() === 'denied' || this.push.isBusy(),
  );

  readonly notificationsHint = computed(() => {
    if (!this.push.isSupported && this.push.needsInstall)
      return 'على الآيفون: اضغط مشاركة ثم "إضافة إلى الشاشة الرئيسية"، وافتح التطبيق من هناك وفعّل الإشعارات';
    if (!this.push.isSupported) return 'المتصفح ده مش بيدعم الإشعارات';
    if (this.push.permission() === 'denied') return 'الإشعارات محظورة من إعدادات المتصفح، فعّلها من هناك الأول';
    return this.notificationsEnabled()
      ? 'هتوصلك الرسايل الجديدة حتى لو الموقع مقفول'
      : 'فعّلها عشان توصلك الرسايل حتى لو الموقع مقفول';
  });

  onBack(): void {
    this.uiService.showChats();
  }

  onToggleTheme(): void {
    this.themeService.toggleTheme();
  }

  onToggleNotifications(): void {
    if (this.notificationsDisabled()) return;
    void (this.notificationsEnabled() ? this.push.disable() : this.push.enable());
  }
}