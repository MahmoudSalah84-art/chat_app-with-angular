import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Subject, firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';
 
const SW_URL = '/push-sw.js';
const ENDPOINT_KEY = 'chatme-push-endpoint';
const SYNCED_AT_KEY = 'chatme-push-synced-at';
const DISMISSED_KEY = 'chatme-push-prompt-dismissed';
const RESYNC_AFTER_MS = 12 * 60 * 60 * 1000;
 
type PushPermission = NotificationPermission | 'unsupported';
 
/**
 * كل حاجة تخص Web Push في الـ Frontend:
 * تسجيل الـ Service Worker، طلب الإذن، الاشتراك، مزامنة الـ subscription مع السيرفر، وتنضيفها عند logout.
 * مكانه: core/services/push.service.ts
 */
@Injectable({ providedIn: 'root' })
export class PushService {
  private readonly http = inject(HttpClient);
  private readonly api = environment.apiUrl;
 
  readonly isSupported =
    typeof window !== 'undefined' &&
    'serviceWorker' in navigator &&
    'PushManager' in window &&
    'Notification' in window;
 
  /** iOS Safari بيدعم Web Push بس للتطبيق المضاف للشاشة الرئيسية (iOS 16.4+) */
  readonly needsInstall = this.detectNeedsInstall();
 
  private readonly _permission = signal<PushPermission>(this.isSupported ? Notification.permission : 'unsupported');
  readonly permission = this._permission.asReadonly();
 
  private readonly _isSubscribed = signal(false);
  readonly isSubscribed = this._isSubscribed.asReadonly();
 
  private readonly _isBusy = signal(false);
  readonly isBusy = this._isBusy.asReadonly();
 
  private readonly _promptDismissed = signal(this.readStorage(DISMISSED_KEY) === '1');
 
  /** البانر بيظهر بس لو المستخدم لسه ما قرّرش (default) ومقفلش البانر قبل كده */
  readonly shouldPrompt = computed(
    () => this.isSupported && this._permission() === 'default' && !this._promptDismissed(),
  );
 
  /** المستخدم داس على notification: افتح الشات ده */
  readonly openChat$ = new Subject<string>();
 
  private registrationPromise: Promise<ServiceWorkerRegistration> | null = null;
  private initialized = false;
 
  // ─────────────────────────── Lifecycle ───────────────────────────
 
  /** بيتنادى مرة واحدة بعد تسجيل الدخول (من MainLayout) */
  async init(): Promise<void> {
    if (!this.isSupported || this.initialized) return;
    this.initialized = true;
 
    navigator.serviceWorker.addEventListener('message', (event) => this.onWorkerMessage(event));
 
    try {
      await this.refreshState();
    } catch (error) {
      console.error('Push init failed', error);
    }
  }
 
  /** لازم يتنادى من click handler (user gesture) عشان المتصفح يسمح بطلب الإذن */
  async enable(): Promise<boolean> {
    if (!this.isSupported || this._isBusy()) return false;
    this._isBusy.set(true);
 
    try {
      const permission = await Notification.requestPermission();
      this._permission.set(permission);
      if (permission !== 'granted') return false;
 
      const registration = await this.getRegistration();
      const subscription = await this.subscribeBrowser(registration);
      await this.syncWithServer(subscription, true);
      this._isSubscribed.set(true);
      return true;
    } catch (error) {
      console.error('Enabling push failed', error);
      return false;
    } finally {
      this._isBusy.set(false);
    }
  }
 
  /** بيوقف الإشعارات على الجهاز ده بس (مش بيسحب إذن المتصفح، ده بيتغيّر من إعدادات المتصفح) */
  async disable(): Promise<void> {
    if (!this.isSupported || this._isBusy()) return;
    this._isBusy.set(true);
 
    try {
      const registration = await this.getRegistration();
      const subscription = await registration.pushManager.getSubscription();
      if (subscription) {
        await this.removeFromServer(subscription.endpoint);
        await subscription.unsubscribe();
      }
      this.clearSyncState();
      this._isSubscribed.set(false);
    } catch (error) {
      console.error('Disabling push failed', error);
    } finally {
      this._isBusy.set(false);
    }
  }
 
  /**
   * بيتنادى من AuthService.logout() قبل مسح التوكن.
   * الطلب لازم يتبعت بشكل synchronous (قبل أي await) عشان الـ authInterceptor يلحق يقرا التوكن،
   * وعشان كده الـ endpoint متخزن في localStorage بدل ما نجيبه من المتصفح.
   */
  cleanupOnLogout(): void {
    if (!this.isSupported) return;
 
    const endpoint = this.readStorage(ENDPOINT_KEY);
    if (endpoint) {
      void firstValueFrom(this.http.post(`${this.api}/push/unsubscribe`, { endpoint })).catch(() => undefined);
    }
 
    this.clearSyncState();
    this._isSubscribed.set(false);
    // نلغي اشتراك المتصفح كمان، عشان المستخدم اللي بعده على نفس الجهاز ميستقبلش رسايل الحساب ده
    void this.unsubscribeBrowser();
  }
 
  dismissPrompt(): void {
    this.writeStorage(DISMISSED_KEY, '1');
    this._promptDismissed.set(true);
  }
 
  // ─────────────────────────── Internals ───────────────────────────
  private getRegistration(): Promise<ServiceWorkerRegistration> {
    this.registrationPromise ??= navigator.serviceWorker.ready;
    return this.registrationPromise;
  }
 
  /** بيطابق الحالة (إذن + اشتراك) مع المتصفح، ويصلّح أي اشتراك ضايع من غير ما يسأل المستخدم تاني */
  private async refreshState(): Promise<void> {
    this._permission.set(Notification.permission);
 
    if (Notification.permission !== 'granted') {
      this._isSubscribed.set(false);
      return;
    }
 
    const registration = await this.getRegistration();
    let subscription = await registration.pushManager.getSubscription();
 
    if (!subscription) {
      // الإذن موجود بس الاشتراك اتمسح (clear site data / rotation): نعيد الاشتراك بصمت
      subscription = await this.subscribeBrowser(registration).catch(() => null);
    }
 
    this._isSubscribed.set(!!subscription);
    if (subscription) await this.syncWithServer(subscription);
  }
 
  private async subscribeBrowser(registration: ServiceWorkerRegistration): Promise<PushSubscription> {
    const { publicKey } = await firstValueFrom(
      this.http.get<{ publicKey: string }>(`${this.api}/push/public-key`),
    );
    const options: PushSubscriptionOptionsInit = {
      userVisibleOnly: true,
      applicationServerKey: this.urlBase64ToUint8Array(publicKey),
    };
 
    try {
      return await registration.pushManager.subscribe(options);
    } 
    catch (error) {
      // لو مفتاح الـ VAPID اتغيّر، الاشتراك القديم بيمنع الجديد: نمسحه ونحاول تاني
      if (error instanceof DOMException && error.name === 'InvalidStateError') {
        await (await registration.pushManager.getSubscription())?.unsubscribe();
        return registration.pushManager.subscribe(options);
      }
      throw error;
    }
  }
 
  /** يسجّل الاشتراك (Upsert) على السيرفر. بنتجنب الطلب لو اتعمل sync من أقل من 12 ساعة لنفس الـ endpoint */
  private async syncWithServer(subscription: PushSubscription, force = false): Promise<void> {
    const json = subscription.toJSON();
    const endpoint = json.endpoint;
    const p256dh = json.keys?.['p256dh'];
    const auth = json.keys?.['auth'];
    if (!endpoint || !p256dh || !auth) return;
 
    const lastSyncedAt = Number(this.readStorage(SYNCED_AT_KEY) ?? 0);
    const isFresh =
      this.readStorage(ENDPOINT_KEY) === endpoint && Date.now() - lastSyncedAt < RESYNC_AFTER_MS;
    if (!force && isFresh) return;
 
    try {
      await firstValueFrom(
        this.http.post(`${this.api}/push/subscribe`, { endpoint, keys: { p256dh, auth } }),
      );
      this.writeStorage(ENDPOINT_KEY, endpoint);
      this.writeStorage(SYNCED_AT_KEY, String(Date.now()));
    } catch (error) {
      console.warn('Push subscription sync failed, will retry on next start', error);
    }
  }
 
  private async removeFromServer(endpoint: string): Promise<void> {
    try {
      await firstValueFrom(this.http.post(`${this.api}/push/unsubscribe`, { endpoint }));
    } catch {
      /* السيرفر هينضّف الـ endpoint لوحده لما يرجع 404/410 */
    }
  }
 
  private async unsubscribeBrowser(): Promise<void> {
    try {
      const registration = await navigator.serviceWorker.getRegistration('/');
      await (await registration?.pushManager.getSubscription())?.unsubscribe();
    } catch {
      /* ignore */
    }
  }
 
  private onWorkerMessage(event: MessageEvent): void {
    const data = event.data;
    if (data?.type === 'chatme:open-chat' && typeof data.chatId === 'string') {
      this.openChat$.next(data.chatId);
    } 
    else if (data?.type === 'chatme:subscription-changed') {
      void this.refreshState();
    }
  }
 
  private clearSyncState(): void {
    try {
      localStorage.removeItem(ENDPOINT_KEY);
      localStorage.removeItem(SYNCED_AT_KEY);
    } catch {
      /* ignore */
    }
  }
 
  private detectNeedsInstall(): boolean {
    if (typeof navigator === 'undefined') return false;
    const isIos =
      /iphone|ipad|ipod/i.test(navigator.userAgent) ||
      (navigator.platform === 'MacIntel' && navigator.maxTouchPoints > 1);
    const isStandalone =
      window.matchMedia('(display-mode: standalone)').matches ||
      (navigator as Navigator & { standalone?: boolean }).standalone === true;
    return isIos && !isStandalone;
  }
 
  private urlBase64ToUint8Array(base64Url: string): Uint8Array<ArrayBuffer> {
    const padding = '='.repeat((4 - (base64Url.length % 4)) % 4);
    const raw = atob((base64Url + padding).replace(/-/g, '+').replace(/_/g, '/'));
    const output = new Uint8Array(raw.length);
    for (let i = 0; i < raw.length; i++) output[i] = raw.charCodeAt(i);
    return output;
  }
 
  private readStorage(key: string): string | null {
    try {
      return localStorage.getItem(key);
    } catch {
      return null;
    }
  }
 
  private writeStorage(key: string, value: string): void {
    try {
      localStorage.setItem(key, value);
    } catch {
      /* ignore */
    }
  }
}