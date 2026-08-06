import { Injectable, signal } from '@angular/core';

const THEME_STORAGE_KEY = 'whatsapp-clone-theme';

/**
 * ThemeService مسؤول بس عن حاجة واحدة: هل التطبيق في Dark Mode ولا لأ.
 * بيحفظ اختيار المستخدم في localStorage عشان يفتكره حتى لو قفل المتصفح.
 */
@Injectable({
  providedIn: 'root',
})
export class ThemeService {
  private readonly _isDarkMode = signal<boolean>(this.getInitialTheme());
  readonly isDarkMode = this._isDarkMode.asReadonly();

  constructor() {
    // نطبّق الوضع فورًا عند تحميل التطبيق
    this.applyThemeToDocument(this._isDarkMode());
  }

  toggleTheme(): void {
    const newValue = !this._isDarkMode();
    this._isDarkMode.set(newValue);
    this.applyThemeToDocument(newValue);
    localStorage.setItem(THEME_STORAGE_KEY, newValue ? 'dark' : 'light');
  }

  /** أول ما التطبيق يفتح: نشوف هل المستخدم اختار حاجة قبل كده، وإلا نتبع إعدادات جهازه */
  private getInitialTheme(): boolean {
    const saved = localStorage.getItem(THEME_STORAGE_KEY);
    if (saved) return saved === 'dark';
    return window.matchMedia('(prefers-color-scheme: dark)').matches;
  }

  /** إضافة أو إزالة class="dark" من عنصر html، وده اللي بيفعّل كل dark: classes */
  private applyThemeToDocument(isDark: boolean): void {
    document.documentElement.classList.toggle('dark', isDark);
  }
}
