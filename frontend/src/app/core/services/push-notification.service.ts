
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { SwPush } from '@angular/service-worker';
import { firstValueFrom } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class PushNotificationService {
  private readonly swPush = inject(SwPush);
  private readonly http = inject(HttpClient);

  private readonly publicKey = 'BKGU4MBCW5-korz6oiDl7MKevSl9rxVxYSNj6Z7AUFV8h0XyaBRBxPJ2Fk_b08XaIdnHBGlSoPdHj8H-zYF53Bo';

  async enableNotifications(): Promise<void> {
    if (!this.swPush.isEnabled) {
      throw new Error('Service Worker is not enabled');
    }

    const subscription = await this.swPush.requestSubscription({
      serverPublicKey: this.publicKey
    });

    await firstValueFrom(
      this.http.post('/api/push-subscriptions', subscription)
    );
  }

  listenToNotificationClicks(): void {
    this.swPush.notificationClicks.subscribe(({ notification }) => {
      const chatId = notification.data?.chatId;

      if (chatId) {
        // غيّر المسار حسب الـ routes الموجودة في مشروعك
        window.location.assign(`/chat/${chatId}`);
      }
    });
  }
}
