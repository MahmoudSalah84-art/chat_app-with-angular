import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Component, ElementRef, inject, OnDestroy, signal, viewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { AuthService, UiService, UserApiService, User, AvatarTypes,  } from '../../../core/index';
import { Avatar } from '../../../shared/components/avatar/avatar';

type EditableField = 'name' | 'about' | null;
const MAX_AVATAR_BYTES = 5 * 1024 * 1024;
const AVATAR_TYPES: string[] = [AvatarTypes.Jpeg, AvatarTypes.Png, AvatarTypes.Webp];

@Component({
  selector: 'app-profile-panel',
  imports: [FormsModule, Avatar],
  templateUrl: './profile-panel.html',
  styleUrl: './profile-panel.css',
})
 
export class ProfilePanel implements OnDestroy {
  private readonly http = inject(HttpClient);
  private readonly authService = inject(AuthService);
  private readonly uiService = inject(UiService);
  private readonly fileInput = viewChild<ElementRef<HTMLInputElement>>('avatarInput');
  private readonly userApi = inject(UserApiService);

  readonly currentUser = this.authService.currentUser;

  readonly editingField = signal<EditableField>(null);
  readonly draftValue = signal('');
  readonly isSaving = signal(false);
  readonly isUploadingAvatar = signal(false);
  readonly avatarError = signal<string | null>(null);
  readonly avatarPreview = signal<string | null>(null);

  onBack(): void {
    this.uiService.showChats();
  }

  ngOnDestroy(): void {
    this.setPreview(null);
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

  onChangeAvatarClick(): void {
    if (this.isUploadingAvatar()) {
      return;
    }

    this.fileInput()?.nativeElement.click();
  }

  async onAvatarSelected(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];

    input.value = '';

    if (!file || this.isUploadingAvatar()) {
      return;
    }

    this.avatarError.set(null);

    if (!AVATAR_TYPES.includes(file.type)) {
      this.avatarError.set(
        'الصورة لازم تكون JPG أو PNG أو WebP'
      );
      return;
    }

    if (file.size === 0 || file.size > MAX_AVATAR_BYTES) {
      this.avatarError.set(
        'حجم الصورة لازم يكون أكبر من صفر وألا يتجاوز 5 ميجا'
      );
      return;
    }

    this.setPreview(URL.createObjectURL(file));

    this.isUploadingAvatar.set(true);

    try {
      const updatedUser = await this.userApi.uploadAvatar(file);

      this.authService.updateStoredUser(updatedUser);
    } catch (error: unknown) {
      console.error('فشل رفع الصورة:', error);

      this.avatarError.set(this.extractError(error));
    } finally {
      this.setPreview(null);

      this.isUploadingAvatar.set(false);
    }
  }


  private setPreview(url: string | null): void {
    const previous = this.avatarPreview();

    if (previous) {
      URL.revokeObjectURL(previous);
    }

    this.avatarPreview.set(url);
  }



  private extractError(error: unknown): string {
    if (error instanceof HttpErrorResponse) {
      if (error.status === 413) {
        return 'حجم الصورة كبير جدًا';
      }

      const errors = error.error?.errors;

      if (Array.isArray(errors)) {
        const first = errors.find(
          (item: unknown) => typeof item === 'string'
        );

        if (typeof first === 'string') {
          return first;
        }
      }

      if (typeof error.error?.message === 'string') {
        return error.error.message;
      }
    }

    return 'فشل رفع الصورة، جرب تاني';
  }
}
 