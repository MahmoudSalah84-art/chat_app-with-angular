import { Component, ElementRef, computed, inject, signal, viewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ChatService } from '../../core/services/chat';
import { UiService } from '../../core/services/ui';
import { Avatar } from '../../shared/components/avatar/avatar';

type Step = 'selectMembers' | 'groupDetails';

/** صورة جروب افتراضية (أيقونة رمادية بسيطة) لو المستخدم مختارش صورة */
const DEFAULT_GROUP_AVATAR =
  'data:image/svg+xml;utf8,' +
  encodeURIComponent(
    `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100"><rect width="100" height="100" fill="%23A0AEC0"/><circle cx="50" cy="38" r="18" fill="white"/><path d="M20 85c0-18 13-32 30-32s30 14 30 32" fill="white"/></svg>`,
  );

/**
 * بانل إنشاء مجموعة جديدة على خطوتين:
 * 1) اختيار الأعضاء من جهات الاتصال (Multi-select)
 * 2) اسم وصورة المجموعة، ثم الإنشاء الفعلي
 */
@Component({
  selector: 'app-new-group',
  imports: [FormsModule, Avatar],
  templateUrl: './new-group.html',
  styleUrl: './new-group.css',
})
export class NewGroup {
  private readonly chatService = inject(ChatService);
  private readonly uiService = inject(UiService);
  private readonly avatarInput = viewChild<ElementRef<HTMLInputElement>>('avatarInput');

  readonly contacts = this.chatService.contacts;
  readonly step = signal<Step>('selectMembers');

  private readonly _searchQuery = signal('');
  private readonly _selectedIds = signal<Set<string>>(new Set());

  readonly groupName = signal('');
  readonly groupAvatar = signal(DEFAULT_GROUP_AVATAR);

  readonly filteredContacts = computed(() => {
    const query = this._searchQuery().trim().toLowerCase();
    if (!query) return this.contacts;
    return this.contacts.filter((c) => c.name.toLowerCase().includes(query));
  });

  readonly selectedIds = this._selectedIds.asReadonly();
  readonly selectedCount = computed(() => this._selectedIds().size);

  readonly selectedContacts = computed(() =>
    this.contacts.filter((c) => this._selectedIds().has(c.id)),
  );

  onSearchChange(value: string): void {
    this._searchQuery.set(value);
  }

  toggleContact(userId: string): void {
    this._selectedIds.update((ids) => {
      const next = new Set(ids);
      if (next.has(userId)) next.delete(userId);
      else next.add(userId);
      return next;
    });
  }

  goToDetailsStep(): void {
    if (this.selectedCount() === 0) return;
    this.step.set('groupDetails');
  }

  backToSelectStep(): void {
    this.step.set('selectMembers');
  }

  onAvatarClick(): void {
    this.avatarInput()?.nativeElement.click();
  }

  onAvatarSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file || !file.type.startsWith('image/')) return;

    const reader = new FileReader();
    reader.onload = () => {
      if (typeof reader.result === 'string') this.groupAvatar.set(reader.result);
    };
    reader.readAsDataURL(file);
    input.value = '';
  }

  onCreateGroup(): void {
    if (!this.groupName().trim()) return;
    this.chatService.createGroup(this.groupName(), this.groupAvatar(), Array.from(this._selectedIds()));
    this.uiService.showChats();
  }

  onClose(): void {
    if (this.step() === 'groupDetails') {
      this.backToSelectStep();
    } else {
      this.uiService.showChats();
    }
  }
}
