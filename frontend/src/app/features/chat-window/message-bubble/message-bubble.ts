import { Component, computed, inject, input, signal } from '@angular/core';
import { Message } from '../../../core/models/message.model';
import { MessageType } from '../../../core/enums/message-type.enum';
import { AuthService } from '../../../core/services/auth.service';
import { ChatFacade } from '../../../core/facade/chat-facade.service';
import { environment } from '../../../../environments/environment';
import { formatBytes } from '../../../core/utils/attachment-rules';
import { getMessageStatus } from '../../../core/utils/message-status';
import { MessageStatus } from '../../../core/enums/message-status.enum';


@Component({
  selector: 'app-message-bubble',
  imports: [],
  templateUrl: './message-bubble.html',
  styleUrl: './message-bubble.css',
})
export class MessageBubble {
  private readonly chatFacade = inject(ChatFacade);
  private readonly authService = inject(AuthService);

  readonly message = input.required<Message>();
  readonly attachment = computed(() => this.message().attachment ?? null);
  readonly imageUrl = computed(() => {
    const url = this.attachment()?.url ?? this.message().content;
    return url.includes('/image/upload/')
      ? url.replace('/image/upload/', '/image/upload/w_600,q_auto,f_auto/')
      : url;
  });
  readonly sizeLabel = computed(() => formatBytes(this.attachment()?.sizeInBytes ?? 0));
  readonly isOwnMessage = computed(() => this.message().senderId === this.authService.currentUser()?.id);
  readonly repliedMessage = computed(() => {
    const replyId = this.message().replyToMessageId;
    if (!replyId) return undefined;
    return this.chatFacade.selectedMessages().find((m) => m.id === replyId);
  });
  readonly time = computed(() =>
    new Date(this.message().sentAt).toLocaleTimeString('ar-EG', { hour: '2-digit', minute: '2-digit' }),
  );
  readonly typeEnum = MessageType;
  readonly isMenuOpen = signal(false);
  readonly isLightboxOpen = signal(false);
  readonly canEdit = computed(
    () => this.isOwnMessage() && this.message().type === this.typeEnum.Text && !this.message().isDeleted,
  );
  readonly canDelete = computed(() => this.isOwnMessage() && !this.message().isDeleted);
  readonly mediaUrl = computed(() => this.resolveMediaUrl(this.message().attachment?.url ?? this.message().content));
  readonly documentData = computed(() => {
    if (this.message().type !== this.typeEnum.File) return null;
    try {
      const parsed = JSON.parse(this.message().content);
      return {
        url: this.resolveMediaUrl(parsed.url || ''),
        fileName: parsed.fileName || 'مستند',
        fileSize: parsed.fileSize ? this.formatFileSize(parsed.fileSize) : '',
      };
    } catch {
      return {
        url: this.resolveMediaUrl(this.message().content),
        fileName: 'مستند',
        fileSize: '',
      };
    }
  });

  readonly statusEnum = MessageStatus;
  readonly status = computed(() =>
    getMessageStatus(this.message(), this.chatFacade.selectedChat(), this.authService.currentUser()?.id ?? ''),
  );

  toggleMenu(): void {
    this.isMenuOpen.update((v) => !v);
  }

  closeMenu(): void {
    this.isMenuOpen.set(false);
  }

  openLightbox(): void {
    this.isLightboxOpen.set(true);
  }

  closeLightbox(): void {
    this.isLightboxOpen.set(false);
  }

  onReplyClick(): void {
    this.chatFacade.setReplyTo(this.message().id);
    this.closeMenu();
  }

  onEditClick(): void {
    this.chatFacade.startEdit(this.message().id);
    this.closeMenu();
  }

  onDeleteClick(): void {
    const confirmed = window.confirm('تحذف الرسالة دي؟ الخطوة دي مش هترجع.');
    if (confirmed) {
      void this.chatFacade.deleteMessage(this.message().id);
    }
    this.closeMenu();
  }

  resolveMediaUrl(url: string): string {
    if (!url) return '';
    if (url.startsWith('http://') || url.startsWith('https://') || url.startsWith('data:')) {
      return url;
    }
    const baseUrl = environment.apiUrl.replace(/\/api\/?$/, '');
    return `${baseUrl}${url.startsWith('/') ? '' : '/'}${url}`;
  }

  formatFileSize(bytes: number): string {
    if (!bytes || bytes === 0) return '0 B';
    const k = 1024;
    const sizes = ['B', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(1)) + ' ' + sizes[i];
  }

  formatRepliedPreview(msg: Message): string {
    switch (msg.type) {
      case this.typeEnum.Image:
        return '📷 صورة';
      case this.typeEnum.Video:
        return '🎥 فيديو';
      case this.typeEnum.Audio:
        return '🎵 مقطع صوتي';
      case this.typeEnum.File:
        return '📄 ملف';
      default:
        return msg.content;
    }
  }



}
