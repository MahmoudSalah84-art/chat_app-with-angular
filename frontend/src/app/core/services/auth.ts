import { Injectable, signal } from '@angular/core';

const AUTH_TOKEN_KEY = 'whatsapp-clone-auth-token';
const AUTH_USER_KEY = 'whatsapp-clone-auth-user';

export interface AuthUser {
  name: string;
  email: string;
}

export interface AuthResult {
  success: boolean;
  errorMessage?: string;
}

/**
 * AuthService مسؤول عن حالة تسجيل الدخول بس (هل المستخدم داخل ولا لأ، وبيانات حسابه).
 *
 * دلوقتي بيحاكي (Mock) عملية تسجيل الدخول محليًا بدون سيرفر حقيقي - بنستخدم
 * setTimeout عشان نمثّل زمن انتظار الشبكة، وبنحفظ "توكن" وهمي في localStorage
 * عشان المستخدم يفضل مسجل دخول حتى لو قفل المتصفح.
 *
 * لما نربط .NET + SignalR بعدين، هنستبدل جوه login/register بس بنداء HTTP حقيقي
 * لـ /api/auth/login اللي هيرجّع JWT حقيقي، من غير ما نغيّر أي Component بيستخدم
 * الـ Service ده.
 */
@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly _isAuthenticated = signal<boolean>(this.hasStoredToken());
  readonly isAuthenticated = this._isAuthenticated.asReadonly();

  private readonly _authUser = signal<AuthUser | null>(this.getStoredUser());
  readonly authUser = this._authUser.asReadonly();

  private readonly _isLoading = signal<boolean>(false);
  readonly isLoading = this._isLoading.asReadonly();

  /** محاكاة تسجيل الدخول - بتقبل أي إيميل صحيح الشكل + كلمة سر 6 حروف فأكتر */
  login(email: string, password: string): Promise<AuthResult> {
    return this.simulateNetworkCall(() => {
      if (!this.isValidEmail(email)) {
        return { success: false, errorMessage: 'صيغة الإيميل مش صحيحة' };
      }
      if (password.length < 6) {
        return { success: false, errorMessage: 'كلمة السر لازم تكون 6 حروف على الأقل' };
      }

      // في نظام حقيقي: السيرفر هو اللي بيتأكد من كلمة السر. هنا بنمثّل نجاح الدخول بس.
      const name = email.split('@')[0];
      this.persistSession({ name, email });
      return { success: true };
    });
  }

  /** محاكاة إنشاء حساب جديد */
  register(name: string, email: string, password: string): Promise<AuthResult> {
    return this.simulateNetworkCall(() => {
      if (!name.trim()) {
        return { success: false, errorMessage: 'من فضلك أدخل اسمك' };
      }
      if (!this.isValidEmail(email)) {
        return { success: false, errorMessage: 'صيغة الإيميل مش صحيحة' };
      }
      if (password.length < 6) {
        return { success: false, errorMessage: 'كلمة السر لازم تكون 6 حروف على الأقل' };
      }

      this.persistSession({ name: name.trim(), email });
      return { success: true };
    });
  }

  logout(): void {
    this._isAuthenticated.set(false);
    this._authUser.set(null);
    localStorage.removeItem(AUTH_TOKEN_KEY);
    localStorage.removeItem(AUTH_USER_KEY);
  }

  /** بيحفظ "جلسة" المستخدم (توكن وهمي + بياناته) بعد نجاح الدخول أو التسجيل */
  private persistSession(user: AuthUser): void {
    const fakeToken = `mock-jwt-${Date.now()}`;
    localStorage.setItem(AUTH_TOKEN_KEY, fakeToken);
    localStorage.setItem(AUTH_USER_KEY, JSON.stringify(user));
    this._authUser.set(user);
    this._isAuthenticated.set(true);
  }

  /** بيلف أي عملية auth بتأخير بسيط عشان يحس المستخدم إن فيه اتصال حقيقي بيحصل */
  private simulateNetworkCall(operation: () => AuthResult): Promise<AuthResult> {
    this._isLoading.set(true);
    return new Promise((resolve) => {
      setTimeout(() => {
        this._isLoading.set(false);
        resolve(operation());
      }, 700);
    });
  }

  private isValidEmail(email: string): boolean {
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
  }

  private hasStoredToken(): boolean {
    return !!localStorage.getItem(AUTH_TOKEN_KEY);
  }

  private getStoredUser(): AuthUser | null {
    const raw = localStorage.getItem(AUTH_USER_KEY);
    return raw ? (JSON.parse(raw) as AuthUser) : null;
  }
}
