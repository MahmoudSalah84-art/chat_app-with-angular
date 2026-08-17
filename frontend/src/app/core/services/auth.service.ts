import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import {User, AuthApiService, AuthOperationResult, AuthResponse} from '../index';


const TOKEN_KEY = 'chatme-token';
const USER_KEY = 'chatme-user';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly authApi = inject(AuthApiService);

  private readonly _isAuthenticated = signal<boolean>(!!this.getStoredToken());
  readonly isAuthenticated = this._isAuthenticated.asReadonly();

  private readonly _currentUser = signal<User | null>(this.getStoredUser());
  readonly currentUser = this._currentUser.asReadonly();

  private readonly _isLoading = signal(false);
  readonly isLoading = this._isLoading.asReadonly();

 
  async register(name: string, email: string, password: string): Promise<AuthOperationResult> {
    return this.performAuthRequest(() => this.authApi.register(name, email, password));
  }

  async login(email: string, password: string): Promise<AuthOperationResult> {
    return this.performAuthRequest(() => this.authApi.login(email, password));
  }

  logout(): void {
    this._isAuthenticated.set(false);
    this._currentUser.set(null);
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
  }

  getToken(): string | null {
    return this.getStoredToken();
  }

  /** بتحدّث بيانات المستخدم المحفوظة محليًا (بعد تعديل البروفايل مثلًا) */
  updateStoredUser(user: User): void {
    this._currentUser.set(user);
    localStorage.setItem(USER_KEY, JSON.stringify(user));
  }

  private async performAuthRequest(request: () => Promise<AuthResponse>): Promise<AuthOperationResult> {
    this._isLoading.set(true);
    try {
      const response = await request();
      localStorage.setItem(TOKEN_KEY, response.token);
      localStorage.setItem(USER_KEY, JSON.stringify(response.user));
      this._currentUser.set(response.user);
      this._isAuthenticated.set(true);
      return { success: true };
    } catch (error: any) {
      const message = error?.error?.errors?.[0] ?? 'حصل خطأ، حاول تاني';
      return { success: false, errorMessage: message };
    } finally {
      this._isLoading.set(false);
    }
  }

  private getStoredToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  private getStoredUser(): User | null {
    const raw = localStorage.getItem(USER_KEY);
    return raw ? (JSON.parse(raw) as User) : null;
  }
}