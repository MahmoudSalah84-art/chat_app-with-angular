import { Component, ElementRef, inject, signal, viewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ChatService } from '../../../core/services/chat';
import { UiService } from '../../../core/services/ui';
import { Avatar } from '../../../shared/components/avatar/avatar';

type EditableField = 'name' | 'about' | null;

/**
 * بانل عرض وتعديل بيانات المستخدم الحالي (الاسم، النبذة، الصورة).
 * بيستبدل قائمة المحادثات مؤقتًا زي ما بيحصل في واتساب بالظبط.
 */
@Component({
  selector: 'app-profile-panel',
  imports: [FormsModule, Avatar],
  templateUrl: './profile-panel.html',
  styleUrl: './profile-panel.css',
})
export class ProfilePanel {
  private readonly chatService = inject(ChatService);
  private readonly uiService = inject(UiService);
  private readonly fileInput = viewChild<ElementRef<HTMLInputElement>>('avatarInput');

  readonly currentUser = this.chatService.currentUser;

  /** أي حقل بيتعدل دلوقتي (null يعني مفيش تعديل جاري) */
  readonly editingField = signal<EditableField>(null);
  readonly draftValue = signal('');

  onBack(): void {
    this.uiService.showChats();
  }

  startEditing(field: EditableField): void {
    if (!field) return;
    this.editingField.set(field);
    this.draftValue.set(field === 'name' ? this.currentUser().name : (this.currentUser().about ?? ''));
  }

  saveEditing(): void {
    const field = this.editingField();
    const value = this.draftValue().trim();
    if (field === 'name' && value) {
      this.chatService.updateProfile({ name: value });
    } else if (field === 'about') {
      this.chatService.updateProfile({ about: value });
    }
    this.editingField.set(null);
  }

  cancelEditing(): void {
    this.editingField.set(null);
  }

  onChangeAvatarClick(): void {
    this.fileInput()?.nativeElement.click();
  }

  onAvatarSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file || !file.type.startsWith('image/')) return;

    const reader = new FileReader();
    reader.onload = () => {
      if (typeof reader.result === 'string') {
        this.chatService.updateProfile({ avatarUrl: reader.result });
      }
    };
    reader.readAsDataURL(file);
    input.value = '';
  }
}
