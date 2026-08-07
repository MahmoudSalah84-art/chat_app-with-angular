import { Injectable, signal } from '@angular/core';

export type SidebarView = 'chats' | 'profile' | 'settings' | 'newChat' | 'newGroup';

/**
 * UiService مسؤول بس عن حاجة واحدة: أي "بانل" ظاهر دلوقتي في الشريط الجانبي
 * (قائمة المحادثات، البروفايل، الإعدادات، أو بدء محادثة جديدة). مفصول عن
 * ChatService عشان ده منطق واجهة (UI State) مش بيانات فعلية.
 */
@Injectable({
  providedIn: 'root',
})
export class UiService {
  private readonly _sidebarView = signal<SidebarView>('chats');
  readonly sidebarView = this._sidebarView.asReadonly();

  showChats(): void {
    this._sidebarView.set('chats');
  }

  showProfile(): void {
    this._sidebarView.set('profile');
  }

  showSettings(): void {
    this._sidebarView.set('settings');
  }

  showNewChat(): void {
    this._sidebarView.set('newChat');
  }

  showNewGroup(): void {
    this._sidebarView.set('newGroup');
  }
}
