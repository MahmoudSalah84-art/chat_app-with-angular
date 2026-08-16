import { HttpClient } from '@angular/common/http';
import { Component, ElementRef, inject, signal, viewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { User } from '../../../core/models/user.model';
import { AuthService } from '../../../core/services/auth.service';
import { UiService } from '../../../core/services/ui.service';
import { Avatar } from '../../../shared/components/avatar/avatar';

type EditableField = 'name' | 'about' | null;

@Component({
  selector: 'app-profile-panel',
  imports: [FormsModule, Avatar],
  templateUrl: './profile-panel.html',
  styleUrl: './profile-panel.css',
})
export class ProfilePanel {
  private readonly http = inject(HttpClient);
  private readonly authService = inject(AuthService);
  private readonly uiService = inject(UiService);
  private readonly fileInput = viewChild<ElementRef<HTMLInputElement>>('avatarInput');

  readonly currentUser = this.authService.currentUser;

  readonly editingField = signal<EditableField>(null);
  readonly draftValue = signal('');
  readonly isSaving = signal(false);

  onBack(): void {
    this.uiService.showChats();
  }

  startEditing(field: EditableField): void {
    if (!field) return;
    const user = this.currentUser();
    if (!user) return;

    this.editingField.set(field);
    this.draftValue.set(field === 'name' ? user.name : (user.about ?? ''));
  }

  async saveEditing(): Promise<void> {
    const field = this.editingField();
    const value = this.draftValue().trim();
    if (!field) return;

    const payload = field === 'name' ? { name: value } : { about: value };
    await this.updateProfile(payload);
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
      if (typeof reader.result === 'string') void this.updateProfile({ avatarUrl: reader.result });
    };
    reader.readAsDataURL(file);
    input.value = '';
  }

  private async updateProfile(payload: { name?: string; about?: string; avatarUrl?: string }): Promise<void> {
    this.isSaving.set(true);
    try {
      const updatedUser = await firstValueFrom(
        this.http.put<User>(`${environment.apiUrl}/users/me`, payload),
      );
      this.authService.updateStoredUser(updatedUser);
    } catch (error) {
      console.error('فشل تحديث البروفايل:', error);
    } finally {
      this.isSaving.set(false);
    }
  }
}
