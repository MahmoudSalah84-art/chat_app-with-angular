import { Component, computed, inject, input, signal } from '@angular/core';
import { Message } from '../../../core/models/message.model';
import { MessageType } from '../../../core/enums/message-type.enum';
import { AuthService } from '../../../core/services/auth.service';
import { ChatFacade } from '../../../core/facade/chat-facade.service';


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

  readonly canEdit = computed(
    () => this.isOwnMessage() && this.message().type === this.typeEnum.Text && !this.message().isDeleted,
  );
  readonly canDelete = computed(() => this.isOwnMessage() && !this.message().isDeleted);

  toggleMenu(): void {
    this.isMenuOpen.update((v) => !v);
  }

  closeMenu(): void {
    this.isMenuOpen.set(false);
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
}
